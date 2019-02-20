using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TemperatureHub.DTOs;
using TemperatureHub.Helpers;
using TemperatureHub.Models;
using TemperatureHub.Process;
using TemperatureHub.Repository;

namespace TemperatureHub.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SettingController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly ISharedData _sharedData = null;
        public SettingController(IMemoryCache cache, ISharedData sharedData)
        {
            _cache = cache;
            _sharedData = sharedData;
        }

        [HttpPost]
        [Route("ClearCache")]
        public ActionResult ClearCache()
        {
            foreach (var item in _sharedData.CacheKey)
            {
                try
                {
                    _cache.Remove(item);
                }
                catch (Exception) { }
            }

            return Ok();
        }

    }
}


