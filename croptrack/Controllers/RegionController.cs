using CropTrack.Models;
using CropTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CropTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        private readonly IRegionService _regionService;

        public RegionController(IRegionService regionService)
        {
            _regionService = regionService;
        }

        // GET api/Region
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            List<Region> regions = await _regionService.GetAllRegions();
            return Ok(regions);
        }

        // GET api/Region/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            Region region = await _regionService.GetRegionById(id);
            if (region == null)
                return NotFound("Region not found.");

            return Ok(region);
        }

        // POST api/Region
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] Region region)
        {
            await _regionService.AddRegion(region);
            return Ok(new { Message = "Region created successfully.", RegionId = region.RegionId });
        }

        // PUT api/Region/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] Region region)
        {
            if (id != region.RegionId)
                return BadRequest("ID mismatch.");

            await _regionService.UpdateRegion(region);
            return Ok("Region updated successfully.");
        }

        // DELETE api/Region/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            bool result = await _regionService.DeleteRegion(id);
            return result ? Ok("Region deleted successfully.") : NotFound("Region not found.");
        }
    }
}
