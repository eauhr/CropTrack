using CropTrack.Models;
using CropTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CropTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WeatherLogController : ControllerBase
    {
        private readonly IWeatherLogService _weatherLogService;
        private readonly IRegionService _regionService;
        private readonly IExternalDataService _externalDataService;

        public WeatherLogController(
            IWeatherLogService weatherLogService,
            IRegionService regionService,
            IExternalDataService externalDataService)
        {
            _weatherLogService = weatherLogService;
            _regionService = regionService;
            _externalDataService = externalDataService;
        }

        // GET api/WeatherLog/{regionId}
        [HttpGet("{regionId}")]
        public async Task<IActionResult> GetByRegion(int regionId)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Region? region = await _regionService.GetRegionById(regionId, farmerId);
            if (region == null)
            {
                return NotFound("Region not found.");
            }

            List<WeatherLog> logs = await _weatherLogService.GetWeatherLogsByRegion(regionId);
            return Ok(logs);
        }

        // GET api/WeatherLog/detail/{id}
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            WeatherLog log = await _weatherLogService.GetWeatherLogById(id);
            if (log == null)
                return NotFound("WeatherLog not found.");

            Region? region = await _regionService.GetRegionById(log.RegionId, farmerId);
            if (region == null)
            {
                return Forbid();
            }

            return Ok(log);
        }

        // POST api/WeatherLog
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WeatherLogModels request)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Region? region = await _regionService.GetRegionById(request.RegionId, farmerId);
            if (region == null)
            {
                return BadRequest("Region not found or does not belong to you.");
            }

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

        // POST api/WeatherLog/fetch-open-meteo/{regionId}
        [HttpPost("fetch-open-meteo/{regionId}")]
        public async Task<IActionResult> FetchOpenMeteo(int regionId, CancellationToken cancellationToken)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Region? region = await _regionService.GetRegionById(regionId, farmerId);
            if (region == null)
            {
                return NotFound("Region not found.");
            }

            OpenMeteoWeatherResult? weather = await _externalDataService.FetchOpenMeteoWeatherAsync(region.Name, cancellationToken);
            if (weather == null)
            {
                return BadRequest($"Could not resolve weather data for region '{region.Name}'.");
            }

            var weatherLog = new WeatherLog
            {
                RegionId = regionId,
                DateRecorded = weather.RecordedAt,
                Temperature = weather.TemperatureC,
                Rainfall = weather.RainfallMm,
                Forecast = weather.Forecast
            };

            await _weatherLogService.AddWeatherLog(weatherLog);

            return Ok(new
            {
                Message = "Weather fetched from Open-Meteo and saved.",
                WeatherLogId = weatherLog.WeatherLogId,
                weather.Latitude,
                weather.Longitude,
                weather.Forecast,
                weather.TemperatureC,
                weather.RainfallMm
            });
        }

        // PUT api/WeatherLog/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WeatherLogModels request)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Region? region = await _regionService.GetRegionById(request.RegionId, farmerId);
            if (region == null)
            {
                return BadRequest("Region not found or does not belong to you.");
            }

            WeatherLog? existing = await _weatherLogService.GetWeatherLogById(id);
            if (existing == null)
            {
                return NotFound("WeatherLog not found.");
            }

            Region? existingRegion = await _regionService.GetRegionById(existing.RegionId, farmerId);
            if (existingRegion == null)
            {
                return Forbid();
            }

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
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            WeatherLog? existing = await _weatherLogService.GetWeatherLogById(id);
            if (existing == null)
            {
                return NotFound("WeatherLog not found.");
            }

            Region? region = await _regionService.GetRegionById(existing.RegionId, farmerId);
            if (region == null)
            {
                return Forbid();
            }

            bool result = await _weatherLogService.DeleteWeatherLog(id);
            return result ? Ok("Weather log deleted successfully.") : NotFound("WeatherLog not found.");
        }
    }
}

