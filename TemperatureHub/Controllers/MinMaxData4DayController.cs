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
    public class MinMaxData4DayController : ControllerBase
    {
        private readonly IProcessData _processData;
        private readonly ISQLiteFileRepository _repository;
        public MinMaxData4DayController(IProcessData processData, ISQLiteFileRepository repository)
        {
            _processData = processData;
            _repository = repository;
        }

        [HttpGet("{id}")]
        public ActionResult<IEnumerable<MinMaxData4DayDTO>> Get(string id, [FromQuery] string from, [FromQuery] string to)
        {
            List<MinMaxData4DayDTO> retData = new List<MinMaxData4DayDTO>();
            List<MinMaxData4Day> minMaxDataList = _repository.LoadMinMaxData4Day(id, from, to);
            foreach (var item in minMaxDataList)
            {
                var tmp = new MinMaxData4DayDTO();
                tmp.MAC = item.SenderMAC;
                tmp.Day = item.Day;
                tmp.Max = Math.Round(item.MaxT, 1);
                tmp.Min = Math.Round(item.MinT, 1);
                tmp.MaxTime = item.MaxTime;
                tmp.MinTime = item.MinTime;

                retData.Add(tmp);
            }

            return retData;
        }

    }
}
