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
    public class MarketPriceController : ControllerBase
    {
        private readonly IMarketPriceService _marketPriceService;
        private readonly ICropService _cropService;
        private readonly IExternalDataService _externalDataService;

        public MarketPriceController(
            IMarketPriceService marketPriceService,
            ICropService cropService,
            IExternalDataService externalDataService)
        {
            _marketPriceService = marketPriceService;
            _cropService = cropService;
            _externalDataService = externalDataService;
        }

        // GET api/MarketPrice/{cropId}
        [HttpGet("{cropId}")]
        public async Task<IActionResult> GetByCrop(int cropId)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Crop? crop = await _cropService.GetCropById(cropId, farmerId);
            if (crop == null)
            {
                return NotFound("Crop not found.");
            }

            List<MarketPrice> prices = await _marketPriceService.GetMarketPricesByCrop(cropId);
            return Ok(prices);
        }

        // GET api/MarketPrice/detail/{id}
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            MarketPrice price = await _marketPriceService.GetMarketPriceById(id);
            if (price == null)
                return NotFound("MarketPrice not found.");

            Crop? crop = await _cropService.GetCropById(price.CropId, farmerId);
            if (crop == null)
            {
                return Forbid();
            }

            return Ok(price);
        }

        // POST api/MarketPrice
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MarketPriceModels request)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Crop? crop = await _cropService.GetCropById(request.CropId, farmerId);
            if (crop == null)
            {
                return BadRequest("Crop not found or does not belong to you.");
            }

            MarketPrice marketPrice = new MarketPrice
            {
                CropId = request.CropId,
                PricePerTon = request.PricePerTon,
                DateRecorded = request.DateRecorded
            };

            await _marketPriceService.AddMarketPrice(marketPrice);
            return Ok(new { Message = "Market price recorded successfully.", MarketPriceId = marketPrice.MarketPriceId });
        }

        // POST api/MarketPrice/fetch-usda/{cropId}
        [HttpPost("fetch-usda/{cropId}")]
        public async Task<IActionResult> FetchUsdaPrice(int cropId, CancellationToken cancellationToken)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Crop? crop = await _cropService.GetCropById(cropId, farmerId);
            if (crop == null)
            {
                return NotFound("Crop not found.");
            }

            UsdaPriceResult? fetchedPrice;
            try
            {
                fetchedPrice = await _externalDataService.FetchUsdaLatestPriceAsync(crop.Name, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            if (fetchedPrice == null)
            {
                return BadRequest($"No USDA price data found for crop '{crop.Name}'.");
            }

            var marketPrice = new MarketPrice
            {
                CropId = cropId,
                PricePerTon = fetchedPrice.Price,
                DateRecorded = DateTime.UtcNow
            };

            await _marketPriceService.AddMarketPrice(marketPrice);

            return Ok(new
            {
                Message = "Market price fetched from USDA and saved.",
                MarketPriceId = marketPrice.MarketPriceId,
                fetchedPrice.Commodity,
                fetchedPrice.Price,
                fetchedPrice.Unit,
                fetchedPrice.Year,
                fetchedPrice.SourceNote
            });
        }

        // PUT api/MarketPrice/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MarketPriceModels request)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Crop? crop = await _cropService.GetCropById(request.CropId, farmerId);
            if (crop == null)
            {
                return BadRequest("Crop not found or does not belong to you.");
            }

            MarketPrice? existing = await _marketPriceService.GetMarketPriceById(id);
            if (existing == null)
            {
                return NotFound("MarketPrice not found.");
            }

            Crop? existingCrop = await _cropService.GetCropById(existing.CropId, farmerId);
            if (existingCrop == null)
            {
                return Forbid();
            }

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
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            MarketPrice? existing = await _marketPriceService.GetMarketPriceById(id);
            if (existing == null)
            {
                return NotFound("MarketPrice not found.");
            }

            Crop? crop = await _cropService.GetCropById(existing.CropId, farmerId);
            if (crop == null)
            {
                return Forbid();
            }

            bool result = await _marketPriceService.DeleteMarketPrice(id);
            return result ? Ok("Market price deleted successfully.") : NotFound("MarketPrice not found.");
        }
    }
}
