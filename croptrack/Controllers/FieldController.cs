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
    public class FieldController : ControllerBase
    {
        private readonly IFieldService _fieldService;

        public FieldController(IFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        [HttpGet("{farmerId}")]
        public async Task<IActionResult> GetAll(int farmerId)
        {
            List<Field> fields = await _fieldService.GetFarmerFields(farmerId);
            return Ok(fields);
        }

        [HttpGet("{farmerId}/{fieldId}")]
        public async Task<IActionResult> Get(int farmerId, int fieldId)
        {
            var field = await _fieldService.GetFieldById(fieldId, farmerId);
            if (field == null) return NotFound("Field not found.");
            return Ok(field);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] FieldModels request)
        {
            var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var field = new Field
            {
                Name = request.Name,
                Location = request.Location,
                Acres = request.Acres,
                RegionId = request.RegionId,
                FarmerId = farmerId
            };

            await _fieldService.AddField(field, farmerId);

            return Ok(new { Message = "Field added successfully", Field = field });
        }


        [HttpPut]
        public async Task<IActionResult> Update([FromBody] FieldModels request)
        {
            var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var field = new Field
            {
                FieldId = request.FieldId,
                Name = request.Name,
                Location = request.Location,
                Acres = request.Acres,
                RegionId = request.RegionId
            };

            var result = await _fieldService.UpdateField(field, farmerId);
            return result ? Ok("Field updated.") : BadRequest("Could not update field.");
        }

        [HttpDelete("{farmerId}/{fieldId}")]
        public async Task<IActionResult> Delete(int farmerId, int fieldId)
        {
            var result = await _fieldService.DeleteField(fieldId, farmerId);
            return result ? Ok("Field deleted.") : BadRequest("Could not delete field.");
        }
    }
}
