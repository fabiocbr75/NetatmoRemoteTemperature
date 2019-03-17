using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using TemperatureHub.Helpers;
using TemperatureHub.Models;
using TemperatureHub.NetatmoData;
using TemperatureHub.Repository;

namespace TemperatureHub.Process
{
    public class ProcessData : IDisposable, IProcessData
    {
        private readonly BlockingCollection<SensorData> _queue = new BlockingCollection<SensorData>();
        private readonly ISQLiteFileRepository _repository = null;
        private readonly INetatmoDataHandler _netatmoCloud = null;
        private readonly ISharedData _sharedData = null;

        private readonly AppSettings _appsettings = null;
        private Thread _executorThd = null;

        public ProcessData(ISQLiteFileRepository repository, INetatmoDataHandler netatmoCloud, IOptions<AppSettings> appSettings, ISharedData sharedData)
        {
            _repository = repository;
            _netatmoCloud = netatmoCloud;
            _appsettings = appSettings.Value;
            _sharedData = sharedData;
            StartExecutionLoop();
        }

        public void Add(SensorData data)
        {
            _queue.Add(data);
        }

        private void StartExecutionLoop()
        {
            _executorThd = new Thread(async() =>
            {
                foreach (var item in _queue.GetConsumingEnumerable())
                {
                    try
                    {
                        AggregateData aggregateData = new AggregateData();

                        var masterData = _repository.LoadSensorMasterData().Where(x => x.SenderMAC == item.SenderMAC).First();

                        aggregateData.IngestionTimestamp = item.IngestionTimestamp;
                        aggregateData.SenderMAC = item.SenderMAC;
                        aggregateData.Temperature = item.Temperature;
                        aggregateData.Humidity = item.Humidity;
                        aggregateData.BatteryLevel = item.BatteryLevel;

                        if (!masterData.ExternalSensor || masterData.NetatmoLinkEnabled)
                        {
                            var token = await _netatmoCloud.GetToken(_appsettings.ClientId, _appsettings.ClientSecret, _appsettings.Username, _appsettings.Password);
                            var schedule = await _netatmoCloud.GetActiveRoomSchedule(_appsettings.HomeId, token.Access_token);
                            if (schedule != null)
                            {
                                var roomScheduled = schedule.RoomSchedules.Where(x => x.RoomId == masterData.RoomId).FirstOrDefault();
                                aggregateData.TScheduledTarget = roomScheduled.TScheduledTarget;

                                var currentStatus = (await _netatmoCloud.GetRoomStatus(_appsettings.HomeId, token.Access_token, schedule.EndTime))?.Where(x => x.RoomId == masterData.RoomId).FirstOrDefault();
                                if (currentStatus != null)
                                {
                                    var newTarget = Number.HalfRound(currentStatus.TValve + roomScheduled.TScheduledTarget - item.Temperature);

                                    aggregateData.TCalculateTarget = newTarget;
                                    aggregateData.TCurrentTarget = currentStatus.TCurrentTarget;
                                    aggregateData.TValve = currentStatus.TValve;

                                    if ((Math.Abs(newTarget - currentStatus.TCurrentTarget) >= 0.5) && masterData.Enabled && !currentStatus.IsAway)
                                    {
                                        var result = await _netatmoCloud.SetThemp(_appsettings.HomeId, currentStatus.RoomId, newTarget, schedule.EndTime, token.Access_token);
                                        aggregateData.SetTempSended = true;
                                        Logger.Message("ProcessData", $"Set NewTarget!!: {result}");
                                    }
                                    Logger.Message("ProcessData", $"Time:{item.IngestionTimestamp} - Room:{masterData.SenderName} - IsAway:{currentStatus.IsAway} - RemoteTemp:{item.Temperature} - ValveTemp:{currentStatus.TValve} - CurrentTarget:{currentStatus.TCurrentTarget} - CalculateTarget: {newTarget} - ScheduledTarget: {roomScheduled.TScheduledTarget} - Humidity:{item.Humidity} - BatteryLevel:{item.BatteryLevel}");
                                }
                                else
                                {
                                    Logger.Warn("ProcessData", $"The is no status for roomid:{masterData.RoomId} - MAC:{masterData.SenderMAC} - Name: {masterData.SenderName}");
                                }
                            }
                            else
                            {
                                Logger.Warn("ProcessData", "schedule is null");
                            }

                        }
                        _sharedData.LastSensorData[item.SenderMAC] = (Temperature: item.Temperature, 
                                                                      IngestionTime: DateTime.ParseExact(item.IngestionTimestamp, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture), 
                                                                      BatteryLevel: item.BatteryLevel, 
                                                                      SenderName: masterData.SenderName, 
                                                                      ScheduledTemperature: aggregateData.TScheduledTarget);

                        _repository.AddAggregateData(aggregateData);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("ProcessData", "Error on execution. " + ex.Message);
                    }
                }
            });

            _executorThd.Start();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _queue.CompleteAdding();

                    _executorThd = null;
                    _queue.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}