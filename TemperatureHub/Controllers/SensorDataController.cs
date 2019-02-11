using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TemperatureHub.DTOs;
using TemperatureHub.Helpers;
using TemperatureHub.Models;
using TemperatureHub.Process;
using TemperatureHub.Repository;

namespace TemperatureHub.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SensorDataController : ControllerBase
    {
        private readonly IProcessData _processData;
        private readonly ISQLiteFileRepository _repository;
        private readonly ISharedData _sharedData = null;
        public SensorDataController(IProcessData processData, ISQLiteFileRepository repository, ISharedData sharedData)
        {
            _processData = processData;
            _repository = repository;
            _sharedData = sharedData;
    }

        [HttpGet("{id}")]
        public ActionResult<IEnumerable<SensorDataExDTO>> Get(string id, [FromQuery] string from, [FromQuery] string to)
        {
            List<SensorDataExDTO> retData = new List<SensorDataExDTO>();
            List<AggregateDataEx> sensorDataList = _repository.LoadSensorDataEx(id, from, to);
            foreach (var item in sensorDataList)
            {
                var tmp = new SensorDataExDTO();
                tmp.MAC = item.SenderMAC;
                tmp.Name = item.SenderName;
                tmp.Temp = item.Temperature;
                tmp.Humidity = item.Humidity;
                tmp.IngestionTimestamp = item.IngestionTimestamp;
                tmp.TValve = item.TValve;
                tmp.TScheduledTarget = item.TScheduledTarget;
                tmp.BatteryLevel = item.BatteryLevel;
                tmp.HeatIndex = HeatHelper.GetHeatIndexCelsius(item.Temperature, item.Humidity);
                tmp.SetTempSended = item.SetTempSended;
                retData.Add(tmp);
            }

            return retData;
        }

        [Route("LastTemperature")]
        public ActionResult<IEnumerable<LastStatusDTO>> LastTemperature()
        {
            return _sharedData.LastSensorData.Select(x => new LastStatusDTO() { MAC = x.Key,
                                                                               Temp = x.Value.Temperature,
                                                                               IngestionTimestamp = x.Value.IngestionTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                                                               BatteryLevel = x.Value.BatteryLevel,
                                                                               SenderName = x.Value.SenderName,
                                                                               ScheduledTemperature = x.Value.ScheduledTemperature }).ToList();
        }

        [Route("LastTemperature/{id}")]
        public ActionResult<LastStatusDTO> LastTemperature(string id)
        {
            return _sharedData.LastSensorData.Where(z => z.Key == id).Select(x => new LastStatusDTO() { MAC = x.Key, Temp = x.Value.Temperature, IngestionTimestamp = x.Value.IngestionTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"), BatteryLevel = x.Value.BatteryLevel, SenderName = x.Value.SenderName, ScheduledTemperature = x.Value.ScheduledTemperature }).FirstOrDefault();
        }

        [HttpPost]
        public void Post([FromBody] SensorDataDTO value)
        {
            var sensorData = new SensorData() { SenderMAC = value.MAC, Temperature = value.Temp, Humidity = value.Humidity, IngestionTimestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), BatteryLevel = value.BatteryLevel };
            _processData.Add(sensorData);
        }
    }
}
