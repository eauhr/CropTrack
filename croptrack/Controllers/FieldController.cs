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
        private readonly IRegionService _regionService;

        public FieldController(IFieldService fieldService, IRegionService regionService)
        {
            _fieldService = fieldService;
            _regionService = regionService;
        }

        [HttpGet("{farmerId}")]
        public async Task<IActionResult> GetAll(int farmerId)
        {
            int tokenFarmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (farmerId != tokenFarmerId)
            {
                return Forbid();
            }

            List<Field> fields = await _fieldService.GetFarmerFields(farmerId);
            return Ok(fields);
        }

        [HttpGet("{farmerId}/{fieldId}")]
        public async Task<IActionResult> Get(int farmerId, int fieldId)
        {
            int tokenFarmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (farmerId != tokenFarmerId)
            {
                return Forbid();
            }

            var field = await _fieldService.GetFieldById(fieldId, farmerId);
            if (field == null) return NotFound("Field not found.");
            return Ok(field);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] FieldModels request)
        {
            var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            int regionId = await ResolveOrCreateRegionIdAsync(request, farmerId, null);

            var field = new Field
            {
                Name = request.Name,
                Location = request.Location,
                Acres = request.Acres,
                RegionId = regionId,
                FarmerId = farmerId
            };

            await _fieldService.AddField(field, farmerId);

            Region? savedRegion = await _regionService.GetRegionById(regionId, farmerId);
            return Ok(new
            {
                Message = "Field added successfully",
                FieldId = field.FieldId,
                field.Name,
                field.Location,
                field.Acres,
                field.RegionId,
                RegionName = savedRegion?.Name ?? string.Empty
            });
        }


        [HttpPut]
        public async Task<IActionResult> Update([FromBody] FieldModels request)
        {
            var farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Field? existing = await _fieldService.GetFieldById(request.FieldId, farmerId);
            if (existing == null)
            {
                return NotFound("Field not found.");
            }

            int regionId = await ResolveOrCreateRegionIdAsync(request, farmerId, existing.RegionId);

            var field = new Field
            {
                FieldId = request.FieldId,
                Name = request.Name,
                Location = request.Location,
                Acres = request.Acres,
                RegionId = regionId
            };

            var result = await _fieldService.UpdateField(field, farmerId);
            return result ? Ok("Field updated.") : BadRequest("Could not update field.");
        }

        [HttpDelete("{farmerId}/{fieldId}")]
        public async Task<IActionResult> Delete(int farmerId, int fieldId)
        {
            int tokenFarmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (farmerId != tokenFarmerId)
            {
                return Forbid();
            }

            var result = await _fieldService.DeleteField(fieldId, farmerId);
            return result ? Ok("Field deleted.") : BadRequest("Could not delete field.");
        }

        private async Task<int> ResolveOrCreateRegionIdAsync(FieldModels request, int farmerId, int? fallbackRegionId)
        {
            if (request.RegionId > 0)
            {
                Region? explicitRegion = await _regionService.GetRegionById(request.RegionId, farmerId);
                if (explicitRegion != null)
                {
                    return explicitRegion.RegionId;
                }
            }

            if (fallbackRegionId.HasValue && fallbackRegionId.Value > 0)
            {
                Region? fallbackRegion = await _regionService.GetRegionById(fallbackRegionId.Value, farmerId);
                if (fallbackRegion != null)
                {
                    return fallbackRegion.RegionId;
                }
            }

            string regionName = ExtractRegionNameFromLocation(request.Location);
            List<Region> existingRegions = await _regionService.GetAllRegions(farmerId);
            Region? matched = existingRegions.FirstOrDefault(r =>
                string.Equals(r.Name, regionName, StringComparison.OrdinalIgnoreCase));

            if (matched != null)
            {
                return matched.RegionId;
            }

            Region created = new()
            {
                Name = regionName
            };

            int createdId = await _regionService.AddRegion(created, farmerId);
            return createdId;
        }

        private static string ExtractRegionNameFromLocation(string? location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return "General Region";
            }

            string[] parts = location
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length >= 3)
            {
                return parts[^2];
            }

            if (parts.Length == 2)
            {
                return parts[0];
            }

            return parts[0];
        }
    }
}
