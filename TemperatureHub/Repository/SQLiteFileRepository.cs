using Microsoft.Extensions.Options;
using Microsoft.Data.Sqlite;
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
        private static ConcurrentDictionary<int, SqliteConnection> _dbInstaces = new ConcurrentDictionary<int, SqliteConnection>();
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

            var connectionString = $"Data Source={fullPath}";
            using (var db = new SqliteConnection(connectionString))
            {
                db.Open();
                CreateOrUpdateTables(db);
                db.Close();
            }
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateDb Get finished");
        }

        private SqliteConnection GetDbInstance()
        {
            SqliteConnection db = null;
            var currentThread = Thread.CurrentThread.ManagedThreadId;

            if (!_dbInstaces.TryGetValue(currentThread, out db))
            {
                var connectionString = $"Data Source={_databasePathWithFileName}";
                db = new SqliteConnection(connectionString);
                db.Open();

                _dbInstaces.AddOrUpdate(currentThread, db, (key, old) => db);
            }

            return db;
        }

        private static void CreateOrUpdateTables(SqliteConnection db)
        {
            Logger.Info("SQLiteFileRepository", "DropAndCreateTables Get started");

            var statements = DbSchemaSQL.SQLScript.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var statement in statements)
            {
                if (!string.IsNullOrWhiteSpace(statement))
                {
                    using (var command = db.CreateCommand())
                    {
                        command.CommandText = statement.Trim();
                        command.ExecuteNonQuery();
                    }
                }
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
                var db = GetDbInstance();
                using (var command = db.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO AggregateData (SenderMAC, Temperature, Humidity, IngestionTimestamp, TValve, TScheduledTarget, TCurrentTarget, BatteryLevel, SetTempSended) 
                                           VALUES (@senderMAC, @temperature, @humidity, @ingestionTimestamp, @tValve, @tScheduledTarget, @tCurrentTarget, @batteryLevel, @setTempSended)";
                    command.Parameters.AddWithValue("@senderMAC", aggregateData.SenderMAC);
                    command.Parameters.AddWithValue("@temperature", aggregateData.Temperature);
                    command.Parameters.AddWithValue("@humidity", aggregateData.Humidity);
                    command.Parameters.AddWithValue("@ingestionTimestamp", aggregateData.IngestionTimestamp);
                    command.Parameters.AddWithValue("@tValve", aggregateData.TValve);
                    command.Parameters.AddWithValue("@tScheduledTarget", aggregateData.TScheduledTarget);
                    command.Parameters.AddWithValue("@tCurrentTarget", aggregateData.TCurrentTarget);
                    command.Parameters.AddWithValue("@batteryLevel", aggregateData.BatteryLevel);
                    command.Parameters.AddWithValue("@setTempSended", aggregateData.SetTempSended);
                    command.ExecuteNonQuery();
                }
            });

            _dataCache[aggregateData.SenderMAC] = aggregateData;
            Logger.Info("SQLiteFileRepository", "AddSensorData Get finished");
        }

        public List<SensorData> LoadSensorData(string mac, string from, string to)
        {
            Logger.Info("SQLiteFileRepository", "LoadSensorData");

            var ret = ExecuteOnThreadPool<List<SensorData>>(() => {
                var result = new List<SensorData>();
                var db = GetDbInstance();
                using (var command = db.CreateCommand())
                {
                    command.CommandText = "SELECT SenderMAC, Temperature, Humidity, IngestionTimestamp FROM AggregateData WHERE SenderMAC = @mac AND IngestionTimestamp BETWEEN @from AND @to ORDER BY IngestionTimestamp";
                    command.Parameters.AddWithValue("@mac", mac);
                    command.Parameters.AddWithValue("@from", from);
                    command.Parameters.AddWithValue("@to", to);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new SensorData
                            {
                                SenderMAC = reader["SenderMAC"].ToString(),
                                Temperature = Convert.ToDouble(reader["Temperature"]),
                                Humidity = Convert.ToDouble(reader["Humidity"]),
                                IngestionTimestamp = reader["IngestionTimestamp"].ToString()
                            });
                        }
                    }
                }
                return result;
            });
            Logger.Info("SQLiteFileRepository", "LoadSensorData Get finished");
            return ret;
        }

        public List<AggregateDataEx> LoadSensorDataEx(string mac, string from, string to)
        {
            Logger.Info("SQLiteFileRepository", "LoadSensorData");

            var ret = ExecuteOnThreadPool<List<AggregateDataEx>>(() => {
                var result = new List<AggregateDataEx>();
                var db = GetDbInstance();
                using (var command = db.CreateCommand())
                {
                    command.CommandText = @"SELECT SD.SenderMAC, SMD.SenderName, SD.Temperature, SD.Humidity, SD.IngestionTimestamp, SD.TValve, SD.TScheduledTarget, SD.TCurrentTarget, SD.BatteryLevel, SD.SetTempSended 
                                           FROM AggregateData SD 
                                           JOIN SensorMasterData SMD ON SMD.SenderMAC = SD.SenderMAC 
                                           WHERE SD.SenderMAC = @mac AND SD.IngestionTimestamp BETWEEN @from AND @to 
                                           ORDER BY SD.IngestionTimestamp";
                    command.Parameters.AddWithValue("@mac", mac);
                    command.Parameters.AddWithValue("@from", from);
                    command.Parameters.AddWithValue("@to", to);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new AggregateDataEx
                            {
                                SenderMAC = reader["SenderMAC"].ToString(),
                                SenderName = reader["SenderName"].ToString(),
                                Temperature = Convert.ToDouble(reader["Temperature"]),
                                Humidity = Convert.ToDouble(reader["Humidity"]),
                                IngestionTimestamp = reader["IngestionTimestamp"].ToString(),
                                TValve = Convert.ToDouble(reader["TValve"]),
                                TScheduledTarget = Convert.ToDouble(reader["TScheduledTarget"]),
                                TCurrentTarget = Convert.ToDouble(reader["TCurrentTarget"]),
                                BatteryLevel = Convert.ToDouble(reader["BatteryLevel"]),
                                SetTempSended = Convert.ToBoolean(reader["SetTempSended"])
                            });
                        }
                    }
                }
                return result;
            });

            Logger.Info("SQLiteFileRepository", "LoadSensorData Get finished");
            return ret;
        }

        public List<SensorMasterData> LoadSensorMasterData()
        {
            Logger.Info("SQLiteFileRepository", "LoadSensorMasterData");

            var ret = ExecuteOnThreadPool<List<SensorMasterData>>(() => {
                var result = new List<SensorMasterData>();
                var db = GetDbInstance();
                using (var command = db.CreateCommand())
                {
                    command.CommandText = "SELECT SenderMAC, SenderName, RoomId, NetatmoSetTemp, ExternalSensor, NetatmoLink FROM SensorMasterData";
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new SensorMasterData
                            {
                                SenderMAC = reader["SenderMAC"].ToString(),
                                SenderName = reader["SenderName"].ToString(),
                                RoomId = reader["RoomId"].ToString(),
                                NetatmoSetTemp = Convert.ToBoolean(reader["NetatmoSetTemp"]),
                                ExternalSensor = Convert.ToBoolean(reader["ExternalSensor"]),
                                NetatmoLink = Convert.ToBoolean(reader["NetatmoLink"])
                            });
                        }
                    }
                }
                return result;
            });
            Logger.Info("SQLiteFileRepository", "LoadSensorMasterData Get finished");
            return ret;
        }

        public List<MinMaxData4Day> LoadMinMaxData4Day(string mac, string from, string to)
        {
            Logger.Info("SQLiteFileRepository", "LoadMinMaxData4Day");
          
            var ret = ExecuteOnThreadPool<List<MinMaxData4Day>>(() => {
                var result = new List<MinMaxData4Day>();
                var db = GetDbInstance();
                using (var command = db.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            *,
                            (SELECT Min(IngestionTimestamp) FROM AggregateData WHERE SenderMAC = @mac AND substr(IngestionTimestamp, 0, 11) = Day AND temperature = MaxT) MaxTime,
                            (SELECT Min(IngestionTimestamp) FROM AggregateData WHERE SenderMAC = @mac AND substr(IngestionTimestamp, 0, 11) = Day AND temperature = MinT) MinTime
                        FROM (
                            SELECT SenderMAC, substr(IngestionTimestamp, 0, 11) as Day, MIN(temperature) as MinT, MAX(temperature) as MaxT 
                            FROM AggregateData 
                            WHERE SenderMAC = @mac AND IngestionTimestamp BETWEEN @from AND @to AND temperature > -10 
                            GROUP BY Day 
                            ORDER BY Day
                        )
                        ORDER BY Day";
                    command.Parameters.AddWithValue("@mac", mac);
                    command.Parameters.AddWithValue("@from", from);
                    command.Parameters.AddWithValue("@to", to);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new MinMaxData4Day
                            {
                                SenderMAC = reader["SenderMAC"].ToString(),
                                Day = reader["Day"].ToString(),
                                MinT = Convert.ToDouble(reader["MinT"]),
                                MaxT = Convert.ToDouble(reader["MaxT"]),
                                MinTime = reader["MinTime"] != DBNull.Value ? reader["MinTime"].ToString() : null,
                                MaxTime = reader["MaxTime"] != DBNull.Value ? reader["MaxTime"].ToString() : null
                            });
                        }
                    }
                }
                return result;
            });
            Logger.Info("SQLiteFileRepository", "LoadMinMaxData4Day Get finished");
            return ret;
        }

        public SensorMasterData SwitchPower(string id, bool power)
        {
            Logger.Info("SQLiteFileRepository", "SwitchPower");

            var ret = ExecuteOnThreadPool<SensorMasterData>(() => {
                var obj = new SensorMasterData();
                var db = GetDbInstance();
                
                using (var command = db.CreateCommand())
                {
                    command.CommandText = "SELECT SenderMAC, SenderName, RoomId, NetatmoSetTemp, ExternalSensor, NetatmoLink FROM SensorMasterData WHERE SenderMAC = @id";
                    command.Parameters.AddWithValue("@id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            obj.SenderMAC = reader["SenderMAC"].ToString();
                            obj.SenderName = reader["SenderName"].ToString();
                            obj.RoomId = reader["RoomId"].ToString();
                            obj.NetatmoSetTemp = Convert.ToBoolean(reader["NetatmoSetTemp"]);
                            obj.ExternalSensor = Convert.ToBoolean(reader["ExternalSensor"]);
                            obj.NetatmoLink = Convert.ToBoolean(reader["NetatmoLink"]);
                        }
                    }
                }
                
                if (obj.SenderMAC != null)
                {
                    obj.NetatmoSetTemp = power;
                    using (var command = db.CreateCommand())
                    {
                        command.CommandText = @"UPDATE SensorMasterData SET NetatmoSetTemp = @power WHERE SenderMAC = @id";
                        command.Parameters.AddWithValue("@power", power);
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                    }
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
                var result = new List<EmailInfo>();
                var db = GetDbInstance();
                using (var command = db.CreateCommand())
                {
                    command.CommandText = "SELECT SmtpServer, SmtpUserName, SmtpPassword, FromMailAddress, ToMailAddress FROM EmailInfo";
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new EmailInfo
                            {
                                SmtpServer = reader["SmtpServer"].ToString(),
                                SmtpUserName = reader["SmtpUserName"].ToString(),
                                SmtpPassword = reader["SmtpPassword"].ToString(),
                                FromMailAddress = reader["FromMailAddress"].ToString(),
                                ToMailAddress = reader["ToMailAddress"].ToString()
                            });
                        }
                    }
                }
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
                var result = new List<WeatherInfo>();
                var db = GetDbInstance();
                using (var command = db.CreateCommand())
                {
                    command.CommandText = @"SELECT SenderMAC, substr(ingestiontimestamp,1,10) AS DAY, min(Temperature) as Min, max(Temperature) as Max 
                                           FROM AggregateData 
                                           WHERE SenderMAC = @mac AND ingestiontimestamp > @firstDay 
                                           GROUP BY SenderMAC, DAY 
                                           ORDER BY DAY DESC";
                    command.Parameters.AddWithValue("@mac", mac);
                    command.Parameters.AddWithValue("@firstDay", firstDay);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new WeatherInfo
                            {
                                SenderMAC = reader["SenderMAC"].ToString(),
                                Day = reader["DAY"].ToString(),
                                Min = Convert.ToDouble(reader["Min"]),
                                Max = Convert.ToDouble(reader["Max"])
                            });
                        }
                    }
                }
                return result;
            });

            Logger.Info("SQLiteFileRepository", "LoadWeatherInfo Get finished");
            return ret;
        }

        // Don't use Dispose pattern for static members. Better specific CleanUp when Application shutdown.
        public static void CleanUp()
        {
            Logger.Info("SQLiteFileRepository", "CleanUp Get started");

            foreach (var item in _dbInstaces)
            {
                SqliteConnection db = null;
                _dbInstaces.TryRemove(item.Key, out db);
                db?.Close();
                db?.Dispose();
            }

            foreach (var item in _executorQueue)
            {
                Executor executor = null;
                if (_executorQueue.TryDequeue(out executor))
                {
                    executor.Run(() => {
                        // Cleanup executor
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

