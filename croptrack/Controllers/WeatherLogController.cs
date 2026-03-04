using CropTrack.Models;
using CropTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CropTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WeatherLogController : ControllerBase
    {
        private readonly IWeatherLogService _weatherLogService;

        public WeatherLogController(IWeatherLogService weatherLogService)
        {
            _weatherLogService = weatherLogService;
        }

        // GET api/WeatherLog/{regionId}
        [HttpGet("{regionId}")]
        public async Task<IActionResult> GetByRegion(int regionId)
        {
            List<WeatherLog> logs = await _weatherLogService.GetWeatherLogsByRegion(regionId);
            return Ok(logs);
        }

        // GET api/WeatherLog/detail/{id}
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            WeatherLog log = await _weatherLogService.GetWeatherLogById(id);
            if (log == null)
                return NotFound("WeatherLog not found.");

            return Ok(log);
        }

        // POST api/WeatherLog
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WeatherLogModels request)
        {
            WeatherLog weatherLog = new WeatherLog
            {
                RegionId = request.RegionId,
                DateRecorded = request.DateRecorded,
                Temperature = request.Temperature,
                Rainfall = request.Rainfall,
                Forecast = request.Forecast
            };

            await _weatherLogService.AddWeatherLog(weatherLog);
            return Ok(new { Message = "Weather log created successfully.", WeatherLogId = weatherLog.WeatherLogId });
        }

        // PUT api/WeatherLog/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WeatherLogModels request)
        {
            WeatherLog weatherLog = new WeatherLog
            {
                WeatherLogId = id,
                RegionId = request.RegionId,
                DateRecorded = request.DateRecorded,
                Temperature = request.Temperature,
                Rainfall = request.Rainfall,
                Forecast = request.Forecast
            };

            bool result = await _weatherLogService.UpdateWeatherLog(weatherLog);
            return result ? Ok("Weather log updated successfully.") : NotFound("WeatherLog not found.");
        }

        // DELETE api/WeatherLog/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool result = await _weatherLogService.DeleteWeatherLog(id);
            return result ? Ok("Weather log deleted successfully.") : NotFound("WeatherLog not found.");
        }
    }
}

