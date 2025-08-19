using CropTrack.Models;
using CropTrack.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CropTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CropController : ControllerBase
    {

        private readonly ICropService _service;

        public CropController(ICropService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
          var crops = _service.GetAllCrops();
            return Ok(crops);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var crop = _service.GetCropById(id);
            if (crop == null) return NotFound();
            return Ok(crop);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CropModels model)
        {
            var crop = new Crop
            {
                Name = model.Name,
                Unit = model.Unit,
                AvgGrowthDays = model.AvgGrowthDays,
                YieldPerAcre = model.YieldPerAcre,
                OptimalTemperature = model.OptimalTemperature
            };

            await _service.AddCrop(crop);

            return Ok(new
            {
                Message = "Crop created successfully",
                CropId = crop.CropId
            });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Crop crop)
        {
            if (id != crop.CropId) return BadRequest("ID mismatch");

            _service.UpdateCrop(crop);
            return Ok("Crop updated successfully");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _service.DeleteCrop(id);
            return Ok("Crop deleted successfully");
        }

    }
}
