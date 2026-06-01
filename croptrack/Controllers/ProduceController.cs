using CropTrack.Models;
using CropTrack.Services;
using Microsoft.AspNetCore.Mvc;

namespace CropTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProduceController : ControllerBase
    {
        private readonly IProduceKnowledgeService _produceKnowledgeService;
        private readonly IWeatherService _weatherService;

        public ProduceController(
            IProduceKnowledgeService produceKnowledgeService,
            IWeatherService weatherService)
        {
            _produceKnowledgeService = produceKnowledgeService;
            _weatherService = weatherService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Produce>>> GetAll(CancellationToken cancellationToken)
        {
            List<Produce> items = await _produceKnowledgeService.GetAllProduces(cancellationToken);
            return Ok(items);
        }

        [HttpGet("recommended")]
        public async Task<ActionResult<List<Produce>>> GetRecommendedByTemperature(
            [FromQuery] double currentTemp,
            CancellationToken cancellationToken)
        {
            List<Produce> items = await _produceKnowledgeService.GetRecommendedCrops(currentTemp, cancellationToken);
            return Ok(items);
        }

        [HttpGet("recommended/by-location")]
        public async Task<IActionResult> GetRecommendedByLocation(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            CancellationToken cancellationToken)
        {
            double? currentTemp = await _weatherService.GetCurrentTemperatureAsync(latitude, longitude, cancellationToken);
            if (!currentTemp.HasValue)
            {
                return BadRequest("Could not fetch temperature from Open-Meteo.");
            }

            List<Produce> items = await _produceKnowledgeService.GetRecommendedCrops(currentTemp.Value, cancellationToken);

            return Ok(new
            {
                currentTempC = currentTemp.Value,
                recommendedCrops = items
            });
        }
    }
}
