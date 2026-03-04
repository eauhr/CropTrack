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
    public class FieldCropController : ControllerBase
    {
        private readonly IFieldCropService _fieldCropService;
        private readonly IFieldService _fieldService;

        public FieldCropController(IFieldCropService fieldCropService, IFieldService fieldService)
        {
            _fieldCropService = fieldCropService;
            _fieldService = fieldService;
        }

        // GET api/FieldCrop/{fieldId}
        [HttpGet("{fieldId}")]
        public async Task<IActionResult> GetAll(int fieldId)
        {
            var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var field = await _fieldService.GetFieldById(fieldId, farmerId);
            if (field == null)
                return NotFound("Field not found or does not belong to you.");

            var fieldCrops = await _fieldCropService.GetFieldCrops(fieldId);
            return Ok(fieldCrops);
        }

        // GET api/FieldCrop/detail/{id}
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var fieldCrop = await _fieldCropService.GetFieldCropById(id);
            if (fieldCrop == null)
                return NotFound("FieldCrop not found.");

            var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var field = await _fieldService.GetFieldById(fieldCrop.FieldId, farmerId);
            if (field == null)
                return Forbid();

            return Ok(fieldCrop);
        }

        // POST api/FieldCrop/add
        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] FieldCropModels request)
        {
            var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var field = await _fieldService.GetFieldById(request.FieldId, farmerId);
            if (field == null)
                return NotFound("Field not found or does not belong to you.");

            var fieldCrop = new FieldCrop
            {
                FieldId = request.FieldId,
                CropId = request.CropId,
                QuantityInTons = request.QuantityInTons,
                PlantingDate = request.PlantingDate,
                HarvestDate = request.HarvestDate
            };

            var result = await _fieldCropService.AddFieldCrop(fieldCrop);
            if (!result)
                return BadRequest("Could not add crop to field.");

            return Ok(new { Message = "Crop added to field successfully.", FieldCropId = fieldCrop.FieldCropId });
        }

        // PUT api/FieldCrop/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FieldCropModels request)
        {
            var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var existing = await _fieldCropService.GetFieldCropById(id);
            if (existing == null)
                return NotFound("FieldCrop not found.");

            var field = await _fieldService.GetFieldById(existing.FieldId, farmerId);
            if (field == null)
                return Forbid();

            var fieldCrop = new FieldCrop
            {
                FieldCropId = id,
                CropId = request.CropId,
                QuantityInTons = request.QuantityInTons,
                PlantingDate = request.PlantingDate,
                HarvestDate = request.HarvestDate
            };

            var result = await _fieldCropService.UpdateFieldCrop(fieldCrop);
            return result ? Ok("FieldCrop updated successfully.") : BadRequest("Could not update FieldCrop.");
        }

        // DELETE api/FieldCrop/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var existing = await _fieldCropService.GetFieldCropById(id);
            if (existing == null)
                return NotFound("FieldCrop not found.");

            var field = await _fieldService.GetFieldById(existing.FieldId, farmerId);
            if (field == null)
                return Forbid();

            var result = await _fieldCropService.DeleteFieldCrop(id);
            return result ? Ok("FieldCrop deleted successfully.") : BadRequest("Could not delete FieldCrop.");
        }
    }
}
