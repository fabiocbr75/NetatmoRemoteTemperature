﻿using Microsoft.Extensions.Options;
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
                        _sharedData.LastSensorData[item.SenderMAC] = (Temperature: item.Temperature, IngestionTime: DateTime.ParseExact(item.IngestionTimestamp, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture), BatteryLevel: item.BatteryLevel);

                        var token = await _netatmoCloud.GetToken(_appsettings.ClientId, _appsettings.ClientSecret, _appsettings.Username, _appsettings.Password);
                        var masterData = _repository.LoadSensorMasterData().Where(x => x.SenderMAC == item.SenderMAC).First();

                        var currentStatus = (await _netatmoCloud.GetRoomStatus(_appsettings.HomeId, token.Access_token))?.Where(x => x.RoomId == masterData.RoomId).FirstOrDefault();
                        if (currentStatus == null)
                        {
                            continue;
                        }
                        var schedule = await _netatmoCloud.GetActiveRoomSchedule(_appsettings.HomeId, token.Access_token);
                        if (schedule == null)
                        {
                            continue;
                        }

                        var roomScheduled = schedule.RoomSchedules.Where(x => x.RoomId == masterData.RoomId).FirstOrDefault();

                        var newTarget = Number.HalfRound(currentStatus.TValve + roomScheduled.TScheduledTarget - item.Temperature);
                        Logger.Message("ProcessData", $"Time:{item.IngestionTimestamp} - Room:{masterData.SenderName} - RemoteTemp:{item.Temperature} - ValveTemp:{currentStatus.TValve} - CurrentTarget:{currentStatus.TCurrentTarget} - CalculateTarget: {newTarget} - ScheduledTarget: {roomScheduled.TScheduledTarget} - Humidity:{item.Humidity} - BatteryLevel:{item.BatteryLevel}");

                        AggregateData aggregateData = new AggregateData()
                        {
                            IngestionTimestamp = item.IngestionTimestamp,
                            SenderMAC = item.SenderMAC,
                            Temperature = item.Temperature,
                            Humidity = item.Humidity,
                            TCalculateTarget = newTarget,
                            TCurrentTarget = currentStatus.TCurrentTarget,
                            TScheduledTarget = roomScheduled.TScheduledTarget,
                            TValve = currentStatus.TValve,
                            SetTempSended = false,
                            BatteryLevel = item.BatteryLevel
                        };

                        if ((Math.Abs(newTarget - currentStatus.TCurrentTarget) >= 0.5) && masterData.Enabled)
                        {
                            //if (newTarget < currentStatus.TCurrentTarget && ((currentStatus.TCurrentTarget - currentStatus.TValve) < 0.5) ||
                            //    newTarget > currentStatus.TCurrentTarget && ((newTarget - currentStatus.TValve) < 0.5)
                            //   )
                            var result = await _netatmoCloud.SetThemp(_appsettings.HomeId, currentStatus.RoomId, newTarget, schedule.EndTime, token.Access_token);
                            aggregateData.SetTempSended = true;
                            Logger.Message("ProcessData", $"Set NewTarget!!: {result}");
                        }

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