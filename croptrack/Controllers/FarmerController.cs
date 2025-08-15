using CropTrack.Data;
using CropTrack.Models;
using CropTrack.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CropTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FarmerController : ControllerBase
    {
        private FarmerService farmerService;

        public FarmerController(FarmerService service)
        {
            farmerService = service;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterFarmerRequest request)
        {
           
                bool result = await farmerService.RegisterAsync(request);
                if (result)
                {
                    return Ok("Farmer registered successfully.");
                }
                else
                {
                    return BadRequest("Farmer could not be registered.");
                }
            }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginFarmerRequest request)
        {
            try
            {
                var farmer = await farmerService.LoginAsync(request);

                return Ok(new
                {
                    Message = "Login successful",
                    Farmer = farmer
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
