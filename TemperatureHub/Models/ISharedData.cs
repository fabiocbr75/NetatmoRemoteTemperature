﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TemperatureHub.Models
{
    public interface ISharedData
    {
        IDictionary<string, (double Temperature, DateTime IngestionTime, double BatteryLevel, string SenderName, double ScheduledTemperature)> LastSensorData { get; }
        ConcurrentQueue<(int ErrorType, string Context, string Message)> LogQueue { get; }
    }
}