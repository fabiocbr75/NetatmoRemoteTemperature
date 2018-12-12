using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TemperatureHub.DTOs;
using TemperatureHub.Models;
using TemperatureHub.Repository;

namespace TemperatureHub.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SensorDataController : ControllerBase
    {
        private readonly ISQLiteFileRepository _repository;
        public SensorDataController(ISQLiteFileRepository repository)
        {
            _repository = repository;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { DateTime.Now.ToLongTimeString() };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] RoomTemp value)
        {
            Console.WriteLine($"Time:{DateTime.Now.ToString("s")} MAC:{value.MAC}; Temp:{value.Temp}; Humidity:{value.Humidity}");

            var sensorData = new SensorData() { senderMAC = value.MAC, temperature = value.Temp, humidity = value.Humidity, ingestionTimestamp = DateTime.UtcNow };
            _repository.AddSensorData(sensorData);
        }
    }
}
