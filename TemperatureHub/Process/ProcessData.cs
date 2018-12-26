using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly AppSettings _appsettings = null;
        private Thread _executorThd = null;

        public ProcessData(ISQLiteFileRepository repository, INetatmoDataHandler netatmoCloud, IOptions<AppSettings> appSettings)
        {
            _repository = repository;
            _netatmoCloud = netatmoCloud;
            _appsettings = appSettings.Value;
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
                        _repository.AddSensorData(item);
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

                        var newTarget = currentStatus.TValve + roomScheduled.TScheduleTarget - item.Temperature;
                        Logger.Message("ProcessData", $"Time:{item.IngestionTimestamp} - Room:{masterData.SenderName} - RemoteTemp:{item.Temperature} - ValveTemp:{currentStatus.TValve} - CurrentTarget:{currentStatus.TCurrentTarget} - CalculateTarget: {newTarget} - ScheduledTarget: {roomScheduled.TScheduleTarget} - Humidity:{item.Humidity}");
                        if ((Math.Abs(newTarget - currentStatus.TCurrentTarget) > 0.6) && masterData.Enabled)
                        {
                            //if (newTarget < currentStatus.TCurrentTarget && ((currentStatus.TCurrentTarget - currentStatus.TValve) < 0.5) ||
                            //    newTarget > currentStatus.TCurrentTarget && ((newTarget - currentStatus.TValve) < 0.5)
                            //   )
                            var result = await _netatmoCloud.SetThemp(_appsettings.HomeId, currentStatus.RoomId, newTarget, schedule.EndTime, token.Access_token);
                            Logger.Message("ProcessData", $"Set NewTarget!!: {result}");
                        }

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