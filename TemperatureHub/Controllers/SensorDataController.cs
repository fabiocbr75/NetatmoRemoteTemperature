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
        public SensorDataController(IProcessData processData, ISQLiteFileRepository repository)
        {
            _processData = processData;
            _repository = repository;
        }

        [HttpGet("{id}")]
        public ActionResult<IEnumerable<SensorDataExDTO>> Get(string id, [FromQuery] string from, [FromQuery] string to)
        {
            List<SensorDataExDTO> retData = new List<SensorDataExDTO>();
            List<SensorDataEx> sensorDataList = _repository.LoadSensorDataEx(id, from, to);
            foreach (var item in sensorDataList)
            {
                var tmp = new SensorDataExDTO();
                tmp.MAC = item.SenderMAC;
                tmp.Name = item.SenderName;
                tmp.Temp = item.Temperature;
                tmp.Humidity = item.Humidity;
                tmp.IngestionTimestamp = item.IngestionTimestamp;
                tmp.HeatIndex = HeatHelper.GetHeatIndexCelsius(item.Temperature, item.Humidity);
                retData.Add(tmp);
            }

            return retData;
        }

        [HttpPost]
        public void Post([FromBody] SensorDataDTO value)
        {
            var sensorData = new SensorData() { SenderMAC = value.MAC, Temperature = value.Temp, Humidity = value.Humidity, IngestionTimestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") };
            _processData.Add(sensorData);
        }
    }
}
