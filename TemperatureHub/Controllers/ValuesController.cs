using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TemperatureHub.Controllers
{
    public class RoomTemp
    {
        public string MAC { get; set; }
        public float Temp { get; set; }
        public float Humidity { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
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
        }
    }
}
