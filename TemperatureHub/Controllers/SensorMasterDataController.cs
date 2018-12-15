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
    public class SensorMasterDataController : ControllerBase
    {
        private readonly ISQLiteFileRepository _repository;
        public SensorMasterDataController(ISQLiteFileRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<SensorMasterDataDTO>> Get()
        {
            List<SensorMasterDataDTO> retData = new List<SensorMasterDataDTO>();
            List<SensorMasterData> sensorMasterDataList = _repository.LoadSensorMasterData();
            foreach (var item in sensorMasterDataList)
            {
                var tmp = new SensorMasterDataDTO();
                tmp.SenderMAC = item.SenderMAC;
                tmp.SenderName= item.SenderName;
                retData.Add(tmp);
            }

            return retData;
        }
    }
}
