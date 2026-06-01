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
    public class RegionController : ControllerBase
    {
        private readonly IRegionService _regionService;

        public RegionController(IRegionService regionService)
        {
            _regionService = regionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            List<Region> regions = await _regionService.GetAllRegions(farmerId);
            return Ok(regions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Region? region = await _regionService.GetRegionById(id, farmerId);
            if (region == null)
            {
                return NotFound("Region not found.");
            }

            return Ok(region);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Region name is required.");
            }

            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Region region = new() { Name = request.Name.Trim() };
            int regionId = await _regionService.AddRegion(region, farmerId);

            return Ok(new { Message = "Region created successfully.", RegionId = regionId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RegionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Region name is required.");
            }

            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            bool updated = await _regionService.UpdateRegion(new Region
            {
                RegionId = id,
                Name = request.Name.Trim()
            }, farmerId);

            return updated ? Ok("Region updated successfully.") : NotFound("Region not found.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int farmerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            bool deleted = await _regionService.DeleteRegion(id, farmerId);
            return deleted ? Ok("Region deleted successfully.") : NotFound("Region not found.");
        }
    }

    public class RegionRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
