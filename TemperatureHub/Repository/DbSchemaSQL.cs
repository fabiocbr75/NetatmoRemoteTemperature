using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TemperatureHub.Repository
{
    public static class DbSchemaSQL
    {
        public const string SQLScript = @"
                CREATE TABLE IF NOT EXISTS SensorData (
                        SenderMAC           TEXT NOT NULL,
                        Temperature         REAL NOT NULL,
                        Humidity            REAL NOT NULL,
                        IngestionTimestamp  TEXT NOT NULL
                );
                
                CREATE UNIQUE INDEX IDX_SensorData ON SensorData (SenderMAC ASC, IngestionTimestamp ASC);

                CREATE TABLE IF NOT EXISTS SensorRoom (
                        SenderMAC           TEXT NOT NULL,
                        RoomId              TEXT NOT NULL
                );

                INSERT INTO SensorRoom (SenderMAC, RoomId) VALUES ('80:7D:3A:57:F2:50', '1541168514');
               ";
    }
}