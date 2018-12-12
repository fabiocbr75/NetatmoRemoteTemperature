using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using TemperatureHub.Helpers;

namespace TemperatureHub.Repository
{
    public class SQLiteFileRepository : ISQLiteFileRepository
    {
        private static ConcurrentDictionary<int, IDbConnection> _dbInstaces = new ConcurrentDictionary<int, IDbConnection>();
        private static ConcurrentQueue<Executor> _executorQueue = new ConcurrentQueue<Executor>();
        
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
            bool needsDatabaseInit = !File.Exists(fullPath);
            using (var db = new SQLiteConnection($"Data Source={fullPath};foreign keys = true;"))
            {
                db.Open();

                if (needsDatabaseInit)
                {
                    DropAndCreateTables(db);
                }
            }
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateDb Get finished");
        }

        private IDbConnection GetDbInstance()
        {
            IDbConnection db = null;
            var currentThread = Thread.CurrentThread.ManagedThreadId;

            if (!_dbInstaces.TryGetValue(currentThread, out db))
            {
                db = new SQLiteConnection($"Data Source={_databasePathWithFileName};foreign keys = true;");
                db.Open();

                _dbInstaces.AddOrUpdate(currentThread, db, (key, old) => db);
            }

            return db;
        }
        /// <summary>Initialiazes a new repository database file</summary>
        private static void DropAndCreateTables(IDbConnection db)
        {
            Logger.Info("SQLiteFileRepository", "DropAndCreateTables Get started");

            db.CreateCommand(DbSchemaSQL.SQLScript).ExecuteNonQuery();

            Logger.Info("SQLiteFileRepository", "DropAndCreateTables Get finished");
        }

        public void AddSensorData(string senderMAC, double temperature, double humidity, DateTime ingestionTimestamp)
        {
            Logger.Info("SQLiteFileRepository", "AddSensorDatastarted");

            string sqlQuery;

            sqlQuery = @"INSERT INTO SensorData (SenderMAC, Temperature, Humidity, IngestionTimestamp)
                              VALUES (@SenderMAC, @Temperature, @Humidity, @IngestionTimestamp) ";


            ExecuteOnThreadPool(() => {
                GetDbInstance().CreateCommand(sqlQuery)
                   .SetParameter("SenderMAC", senderMAC)
                   .SetParameter("Temperature", Math.Round(temperature, 1))
                   .SetParameter("Humidity", Math.Round(humidity, 1))
                   .SetParameter("IngestionTimestamp", ingestionTimestamp.ToString("yyyy-MM-ddTHH:mm:ssZ"))
                   .ExecuteNonQuery();

            });
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateUserRole Get finished");
        }



        /*
		
        public IEnumerable<DataSourceItem> GetDataSourcesFromDb(Guid datasourceId)
        {
            Logger.Info("SQLiteFileRepository", "GetDataSourcesFromDb Get started");

            var ret = ExecuteOnThreadPool<IEnumerable<DataSourceItem>>(() =>
            {
                List<DataSourceItem> result = new List<DataSourceItem>();

                var cmd = GetDbInstance().CreateCommand("SELECT DataSourceId, DataSourceType, DataSourceName, ServerName, DatabaseName, UserName, Password, UseWindowsAuth, Description FROM DataSources");

                if (datasourceId != default(Guid))
                {
                    cmd.CommandText += " WHERE (DataSourceId = @DataSourceId)";
                    cmd.SetParameter("DataSourceId", datasourceId.ToString().ToUpperInvariant());
                }

                cmd.CommandText += " ORDER BY DataSourceName";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bool useWinAuth = reader.GetInt32(reader.GetOrdinal("UseWindowsAuth")) != 0;
                        string decryptPwd = string.Empty;
                        var encryptPwd = reader.GetString(reader.GetOrdinal("Password"));
                        if (!string.IsNullOrEmpty(encryptPwd))
                        {
                            decryptPwd = Unprotect(encryptPwd ?? "", "", DataProtectionScope.LocalMachine);
                        }

                        result.Add(new DataSourceItem(
                            Guid.Parse(reader.GetString(reader.GetOrdinal("DataSourceId"))),
                            reader.GetString(reader.GetOrdinal("DataSourceName")),
                            (DataSourceType)reader.GetInt32(reader.GetOrdinal("DataSourceType")),
                            reader.GetString(reader.GetOrdinal("ServerName")),
                            reader.GetString(reader.GetOrdinal("DatabaseName")),
                            reader.GetString(reader.GetOrdinal("UserName")),
                            decryptPwd,
                            useWinAuth,
                            reader.GetString(reader.GetOrdinal("Description"))
                            )
                        );
                    }
                }
                return result;
            });

            Logger.Info("SQLiteFileRepository", "GetDataSourcesFromDb Get finished");
            return ret;
        }
        */
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
                        IDbConnection db = null;
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

