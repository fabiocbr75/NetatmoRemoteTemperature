using Microsoft.Extensions.Options;
using SQLite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using TemperatureHub.Helpers;
using TemperatureHub.Models;

namespace TemperatureHub.Repository
{
    public class SQLiteFileRepository : ISQLiteFileRepository
    {
        private static ConcurrentDictionary<int, SQLiteConnection> _dbInstaces = new ConcurrentDictionary<int, SQLiteConnection>();
        private static ConcurrentQueue<Executor> _executorQueue = new ConcurrentQueue<Executor>();
        private Dictionary<string, AggregateData> _dataCache = new Dictionary<string, AggregateData>();
        
        private readonly string _databasePathWithFileName;

        public SQLiteFileRepository() { }

        public SQLiteFileRepository(IOptions<AppSettings> appSettings)
        {
            _databasePathWithFileName = Path.Combine(appSettings.Value.DbFullPath, "repository.db");
        }

        public static void CreateOrUpdateDb(string databasePath)
        {
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateDb Get started");

            System.IO.Directory.CreateDirectory(databasePath);
            var fullPath = Path.Combine(databasePath, "repository.db");

            using (var db = new SQLiteConnection(fullPath))
            {
                CreateOrUpdateTables(db);
            }
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateDb Get finished");
        }

        private SQLiteConnection GetDbInstance()
        {
            SQLiteConnection db = null;
            var currentThread = Thread.CurrentThread.ManagedThreadId;

            if (!_dbInstaces.TryGetValue(currentThread, out db))
            {
                db = new SQLiteConnection(_databasePathWithFileName);

                _dbInstaces.AddOrUpdate(currentThread, db, (key, old) => db);
            }

            return db;
        }

        private static void CreateOrUpdateTables(SQLiteConnection db)
        {
            Logger.Info("SQLiteFileRepository", "DropAndCreateTables Get started");

            var statements = DbSchemaSQL.SQLScript.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var statement in statements)
            {
                db.Execute(statement);
            }

            Logger.Info("SQLiteFileRepository", "DropAndCreateTables Get finished");
        }

        public void AddAggregateData(AggregateData aggregateData)
        {
            Logger.Info("SQLiteFileRepository", "AddSensorData Get started");
            AggregateData lastData;

            if (_dataCache.TryGetValue(aggregateData.SenderMAC, out lastData))
            {
                if (aggregateData.Temperature == lastData.Temperature && aggregateData.Humidity == lastData.Humidity && 
                    aggregateData.BatteryLevel == lastData.BatteryLevel && lastData.TScheduledTarget == aggregateData.TScheduledTarget &&
                    aggregateData.TValve == lastData.TValve)
                {
                    Logger.Info("SQLiteFileRepository", "AddSensorData Data skipped");
                    Logger.Info("SQLiteFileRepository", "AddSensorData Get finished");
                    return;
                }
            }

            ExecuteOnThreadPool(() => {
                GetDbInstance().Insert(aggregateData);
            });

            _dataCache[aggregateData.SenderMAC] = aggregateData;
            Logger.Info("SQLiteFileRepository", "AddSensorDatastarted Get finished");
        }

        public List<SensorData> LoadSensorData(string mac, string from, string to)
        {
            Logger.Info("SQLiteFileRepository", "LoadSensorData");

            var ret = ExecuteOnThreadPool<List<SensorData>>(() => {
                var result = GetDbInstance().Query<SensorData>("SELECT SenderMAC, Temperature, Humidity, IngestionTimestamp FROM AggregateData WHERE SenderMAC = ? AND IngestionTimestamp BETWEEN ? AND ? ORDER BY IngestionTimestamp", mac, from, to);
                return result;
            });
            Logger.Info("SQLiteFileRepository", "LoadSensorData Get finished");
            return ret;
        }

        public List<AggregateDataEx> LoadSensorDataEx(string mac, string from, string to)
        {
            Logger.Info("SQLiteFileRepository", "LoadSensorData");

            var ret = ExecuteOnThreadPool<List<AggregateDataEx>>(() => {
                var result = GetDbInstance().Query<AggregateDataEx>("SELECT SD.SenderMAC, SMD.SenderName, SD.Temperature, SD.Humidity, SD.IngestionTimestamp, SD.TValve, SD.TScheduledTarget, SD.TCurrentTarget, SD.BatteryLevel, SD.SetTempSended FROM AggregateData SD JOIN SensorMasterData SMD ON  SMD.SenderMAC = SD.SenderMAC WHERE SD.SenderMAC = ? AND SD.IngestionTimestamp BETWEEN ? AND ? ORDER BY SD.IngestionTimestamp", mac, from, to);
                return result;
            });

            Logger.Info("SQLiteFileRepository", "LoadSensorData Get finished");
            return ret;
        }

        public List<SensorMasterData> LoadSensorMasterData()
        {
            Logger.Info("SQLiteFileRepository", "LoadSensorMasterData");

            var ret = ExecuteOnThreadPool<List<SensorMasterData>>(() => {
                var result = GetDbInstance().Query<SensorMasterData>("SELECT SenderMAC, SenderName, RoomId, NetatmoSetTemp, ExternalSensor FROM SensorMasterData");
                return result;
            });
            Logger.Info("SQLiteFileRepository", "LoadSensorMasterData Get finished");
            return ret;
        }
        public SensorMasterData SwitchPower(string id, bool power)
        {
            Logger.Info("SQLiteFileRepository", "SwitchPower");

            var ret = ExecuteOnThreadPool<SensorMasterData>(() => {
                var obj = GetDbInstance().Query<SensorMasterData>("SELECT SenderMAC, SenderName, RoomId, NetatmoSetTemp, ExternalSensor FROM SensorMasterData WHERE SenderMAC = ?", id).FirstOrDefault();
                if (obj != null)
                {
                    obj.NetatmoSetTemp = power;
                    var result = GetDbInstance().Update(obj);
                }
                return obj;
            });

            Logger.Info("SQLiteFileRepository", "SwitchPower Get finished");
            return ret;
        }
        public List<EmailInfo> LoadEmailInfo()
        {
            Logger.Info("SQLiteFileRepository", "LoadEmailInfo");

            var ret = ExecuteOnThreadPool<List<EmailInfo>>(() => {
                var result = GetDbInstance().Query<EmailInfo>("SELECT SmtpServer, SmtpUserName, SmtpPassword, FromMailAddress, ToMailAddress FROM EmailInfo");
                return result;
            });
        
        Logger.Info("SQLiteFileRepository", "LoadEmailInfo Get finished");
            return ret;
        }
        public List<WeatherInfo> LoadWeatherInfo(string mac, int lastDays)
        {
            Logger.Info("SQLiteFileRepository", "LoadWeatherInfo");

            var firstDay = DateTime.UtcNow.Subtract(TimeSpan.FromDays(lastDays-1)).ToString("yyyy-MM-ddT00:00:00Z");

            var ret = ExecuteOnThreadPool<List<WeatherInfo>>(() => {
                var result = GetDbInstance().Query<WeatherInfo>($"select SenderMAC, substr(ingestiontimestamp,1,10) AS DAY, min(Temperature) as Min, max(Temperature) as Max from AggregateData where SenderMAC = '{mac}' and ingestiontimestamp > '{firstDay}' group by SenderMAC, DAY ORDER BY DAY DESC;");
                return result;
            });

            Logger.Info("SQLiteFileRepository", "LoadWeatherInfo Get finished");
            return ret;
        }

        // Don't use Dispose pattern for static members. Better specific CleanUp when Application shutdown.
        public static void CleanUp()
        {
            Logger.Info("SQLiteFileRepository", "CleanUp Get started");

            foreach (var item in _executorQueue)
            {
                Executor executor = null;
                if (_executorQueue.TryDequeue(out executor))
                {
                    executor.Run(() => {
                        SQLiteConnection db = null;
                        var currentThread = Thread.CurrentThread.ManagedThreadId;
                        _dbInstaces.TryRemove(currentThread, out db);
                        db?.Close();
                        db?.Dispose();
                    });
                    executor.Dispose();
                }
            }
            // It is needed to release file lock
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Logger.Info("SQLiteFileRepository", "CleanUp Get finished");
        }

        private Executor GetExecutor()
        {
            Executor executor;
            if (!_executorQueue.TryDequeue(out executor))
            {
                executor = new Executor();
                executor.StartExecutionLoop();
            }

            return executor;
        }

        private void ExecuteOnThreadPool(Action action)
        {
            Executor executor = GetExecutor();

            try
            {
                executor.Run(() => { action(); });
                _executorQueue.Enqueue(executor);
            }
            catch (Exception ex)
            {
                Logger.Error("SQLiteFileRepository", "ExecuteOnThreadPool Action Error. " + ex.Message);
            }
        }

        private T ExecuteOnThreadPool<T>(Func<T> func)
        {
            T ret = default(T);
            Executor executor = GetExecutor();

            try
            {
                executor.Run(() => { ret = func(); });
                _executorQueue.Enqueue(executor);
            }
            catch (Exception ex)
            {
                Logger.Error("SQLiteFileRepository", "ExecuteOnThreadPool Func Error. " + ex.Message);
            }
            return ret;
        }
    }
}

