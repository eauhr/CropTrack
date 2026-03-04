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
    public class MarketPriceController : ControllerBase
    {
        private readonly IMarketPriceService _marketPriceService;

        public MarketPriceController(IMarketPriceService marketPriceService)
        {
            _marketPriceService = marketPriceService;
        }

        // GET api/MarketPrice/{cropId}
        [HttpGet("{cropId}")]
        public async Task<IActionResult> GetByCrop(int cropId)
        {
            List<MarketPrice> prices = await _marketPriceService.GetMarketPricesByCrop(cropId);
            return Ok(prices);
        }

        // GET api/MarketPrice/detail/{id}
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            MarketPrice price = await _marketPriceService.GetMarketPriceById(id);
            if (price == null)
                return NotFound("MarketPrice not found.");

            return Ok(price);
        }

        // POST api/MarketPrice
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MarketPriceModels request)
        {
            MarketPrice marketPrice = new MarketPrice
            {
                CropId = request.CropId,
                PricePerTon = request.PricePerTon,
                DateRecorded = request.DateRecorded
            };

            await _marketPriceService.AddMarketPrice(marketPrice);
            return Ok(new { Message = "Market price recorded successfully.", MarketPriceId = marketPrice.MarketPriceId });
        }

        // PUT api/MarketPrice/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MarketPriceModels request)
        {
            MarketPrice marketPrice = new MarketPrice
            {
                MarketPriceId = id,
                CropId = request.CropId,
                PricePerTon = request.PricePerTon,
                DateRecorded = request.DateRecorded
            };

            bool result = await _marketPriceService.UpdateMarketPrice(marketPrice);
            return result ? Ok("Market price updated successfully.") : NotFound("MarketPrice not found.");
        }

        // DELETE api/MarketPrice/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool result = await _marketPriceService.DeleteMarketPrice(id);
            return result ? Ok("Market price deleted successfully.") : NotFound("MarketPrice not found.");
        }
    }
}
