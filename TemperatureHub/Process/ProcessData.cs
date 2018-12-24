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
        private ISQLiteFileRepository _repository = null;
        private INetatmoDataHandler _netatmoCloud = null;
        private Thread _executorThd = null;

        public ProcessData(ISQLiteFileRepository repository, INetatmoDataHandler netatmoCloud)
        {
            _repository = repository;
            _netatmoCloud = netatmoCloud;
            StartExecutionLoop();
        }

        public void Add(SensorData data)
        {
            _queue.Add(data);
        }

        private void StartExecutionLoop()
        {
            _executorThd = new Thread(() =>
            {
                foreach (var item in _queue.GetConsumingEnumerable())
                {
                    try
                    {
                        _repository.AddSensorData(item);






                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Executor", "Error on execution. " + ex.Message);
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