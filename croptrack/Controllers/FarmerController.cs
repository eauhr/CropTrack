using croptrack.Data;
using croptrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace croptrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FarmerController : ControllerBase
    {
        private readonly FieldTrackContext _context;

        public FarmerController(FieldTrackContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(Farmer farmer)
        {
            if (await _context.Farmers.AnyAsync(f => f.Email == farmer.Email))
            {
                return BadRequest("Email already registered.");
            }

            _context.Farmers.Add(farmer);
            await _context.SaveChangesAsync();

            return Ok("Farmer registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Farmer loginData)
        {
            var farmer = await _context.Farmers
                .FirstOrDefaultAsync(f => f.Email == loginData.Email && f.Password == loginData.Password);

            if (farmer == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            return Ok($"Welcome {farmer.Name}!");
        }



    }
}
