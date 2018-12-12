using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using TemperatureHub.Helpers;

namespace TemperatureHub.Repository
{
    public class Executor : IDisposable
    {
        private readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
        private AutoResetEvent _signal = new AutoResetEvent(false);
        private AutoResetEvent _disposed = new AutoResetEvent(false);
        private Thread _executorThd = null;
        private const int _timeout = 10000;

        public Executor()
        {
        }

        public void Run(Action action)
        {
            _queue.Add(action);
            if (!_signal.WaitOne(_timeout))
            {
                Logger.Warn("Executor", "Executor Timeout");
                throw new ApplicationException("Executor Timeout");
            }
        }

        public void StartExecutionLoop()
        {
            _executorThd = new Thread(() =>
            {
                foreach (var item in _queue.GetConsumingEnumerable())
                {
                    try
                    {
                        item();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Executor", "Error on execution. " + ex.Message);
                    }
                    finally
                    {
                        _signal.Set();
                    }
                }
                _disposed.Set();
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

                    _disposed.WaitOne(_timeout);
                    _executorThd = null;
                    _queue.Dispose();
                    _signal.Dispose();
                    _disposed.Dispose();

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