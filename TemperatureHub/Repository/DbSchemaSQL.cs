using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TemperatureHub.Repository
{
    public static class DbSchemaSQL
    {
        public const string SQLScript = @"
                CREATE TABLE IF NOT EXISTS SensorMasterData (
                        SenderMAC           TEXT NOT NULL PRIMARY KEY,
                        SenderName          TEXT NOT NULL COLLATE NOCASE,
                        RoomId              TEXT NOT NULL COLLATE NOCASE,
                        Enabled             INTEGER NOT NULL,
                        ExternalSensor      INTEGER NOT NULL DEFAULT 0
                );
                INSERT OR IGNORE INTO SensorMasterData VALUES ('80:7D:3A:47:5B:62', 'Cucina', '2809735084', 1, 0);
                INSERT OR IGNORE INTO SensorMasterData VALUES ('80:7D:3A:47:5C:B2', 'Sala', '2935863693', 1, 0);
                INSERT OR IGNORE INTO SensorMasterData VALUES ('80:7D:3A:47:5C:C5', 'Camera', '3716460054', 1, 0);
                INSERT OR IGNORE INTO SensorMasterData VALUES ('80:7D:3A:47:59:86', 'Cameretta', '3702889680', 1, 0);
                INSERT OR IGNORE INTO SensorMasterData VALUES ('84:F3:EB:0D:BC:23', 'Bagno', '3575883469', 1, 0);
                INSERT OR IGNORE INTO SensorMasterData VALUES ('80:7D:3A:57:F2:50', 'Genoa', '0000000000', 0, 1);




                CREATE TABLE IF NOT EXISTS AggregateData (
                        SenderMAC           TEXT NOT NULL,
                        Temperature         REAL NOT NULL,
                        Humidity            REAL NOT NULL,
                        IngestionTimestamp  TEXT NOT NULL,
                        TValve              REAL NOT NULL,
                        TCurrentTarget      REAL NOT NULL,
                        TCalculateTarget    REAL NOT NULL,
                        TScheduledTarget    REAL NOT NULL,
                        SetTempSended       INT  NOT NULL,
                        BatteryLevel        REAL NOT NULL,
                        FOREIGN KEY (SenderMAC) REFERENCES SensorMasterData(SenderMAC) ON DELETE CASCADE
                );
                
                CREATE UNIQUE INDEX IF NOT EXISTS IDX_AggregateData ON AggregateData (SenderMAC ASC, IngestionTimestamp ASC);

                CREATE TABLE IF NOT EXISTS EmailInfo(
                    SmtpServer      TEXT NOT NULL,
                    SmtpUserName    TEXT NOT NULL,
                    SmtpPassword    TEXT NOT NULL,
                    FromMailAddress TEXT NOT NULL,
                    ToMailAddress   TEXT NOT NULL
                );";
    }
}