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
    public class CropController : ControllerBase
    {
        private readonly ICropService _service;

        public CropController(ICropService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            List<Crop> crops = await _service.GetAllCrops(farmerId);
            return Ok(crops);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Crop? crop = await _service.GetCropById(id, farmerId);
            if (crop == null)
            {
                return NotFound();
            }

            return Ok(crop);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CropModels model)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            Crop crop = new()
            {
                Name = model.Name,
                Unit = model.Unit,
                AvgGrowthDays = model.AvgGrowthDays,
                YieldPerAcre = model.YieldPerAcre,
                OptimalTemperature = model.OptimalTemperature
            };

            int cropId = await _service.AddCrop(crop, farmerId);

            return Ok(new
            {
                Message = "Crop created successfully",
                CropId = cropId
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Crop crop)
        {
            if (id != crop.CropId)
            {
                return BadRequest("ID mismatch");
            }

            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            bool updated = await _service.UpdateCrop(crop, farmerId);
            return updated ? Ok("Crop updated successfully") : NotFound("Crop not found.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            bool deleted = await _service.DeleteCrop(id, farmerId);
            return deleted ? Ok("Crop deleted successfully") : NotFound("Crop not found.");
        }
    }
}
