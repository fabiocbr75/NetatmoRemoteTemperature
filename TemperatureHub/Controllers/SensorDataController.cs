using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TemperatureHub.DTOs;
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
        public ActionResult<IEnumerable<RoomTemp>> Get(string id, [FromQuery] string from, [FromQuery] string to)
        {
            List<RoomTemp> retData = new List<RoomTemp>();
            List<SensorData> sensorDataList = _repository.LoadSensorData(id, from, to);
            foreach (var item in sensorDataList)
            {
                var tmp = new RoomTemp();
                tmp.MAC = item.SenderMAC;
                tmp.Temp = item.Temperature;
                tmp.Humidity = item.Humidity;
                tmp.IngestionTimestamp = item.IngestionTimestamp;
                retData.Add(tmp);
            }

            return retData;
        }

        [HttpPost]
        public void Post([FromBody] RoomTemp value)
        {
            Console.WriteLine($"Time:{DateTime.Now.ToString("s")} MAC:{value.MAC}; Temp:{value.Temp}; Humidity:{value.Humidity}");

            var sensorData = new SensorData() { SenderMAC = value.MAC, Temperature = value.Temp, Humidity = value.Humidity, IngestionTimestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") };
            _processData.Add(sensorData);
        }
    }
}
