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
    public class WeatherController : ControllerBase
    {
        private readonly ISQLiteFileRepository _repository;
        private readonly ISharedData _sharedData = null;
        public WeatherController(ISQLiteFileRepository repository, ISharedData sharedData)
        {
            _repository = repository;
            _sharedData = sharedData;
        }

        [HttpGet("{id}")]
        public ActionResult<WeatherDTO> Get(string id)
        {
            const int days = 5;
            var ret = new WeatherDTO();

            try
            {

                var weatherInfoList = _repository.LoadWeatherInfo(id, days);
                var last = _sharedData.LastSensorData.Where(z => z.Key == id).FirstOrDefault();
                ret.SenderMAC = id;
                ret.SenderName = last.Value.SenderName ?? "";
                ret.Temperature = last.Value.Temperature;
                ret.Humidity = last.Value.Humidity;
                ret.Date = DateTime.Now.ToString("ddd, dd MMM");

                var weatherInfoHistory = new WeatherInfoDTO[days];

                int i = 0;
                foreach (var item in weatherInfoList)
                {
                    weatherInfoHistory[i] = new WeatherInfoDTO();
                    weatherInfoHistory[i].Max = item.Max;
                    weatherInfoHistory[i].Min = item.Min;
                    weatherInfoHistory[i].DateISO = item.Day;
                    weatherInfoHistory[i].DateOfWeek = DateTime.Parse(weatherInfoHistory[i].DateISO).ToString("ddd");
                    i++;
                }

                for (int y = i; y < days; y++)
                {
                    weatherInfoHistory[y] = new WeatherInfoDTO();
                }

                ret.WeatherInfo = weatherInfoHistory;
            }
            catch (Exception ex)
            {
                Logger.Error("ProcessData", "Error on execution. " + ex.Message);
            }

            return ret;
        }

    }
}


