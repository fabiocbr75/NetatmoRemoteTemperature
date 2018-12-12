using System;
using System.Data;

namespace TemperatureHub.Repository
{
    internal static class IDbCommandExtensions
    {

        public static IDbCommand CreateCommand(this IDbConnection connection, string sql)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            return cmd;
        }

        public static IDbCommand SetParameter(this IDbCommand cmd, string paramName, object paramValue)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = paramName;
            if (paramValue != null)
            {
                param.Value = paramValue;
            }
            else
            {
                param.Value = DBNull.Value;
            }
            cmd.Parameters.Add(param);
            return cmd;
        }
    }
}

