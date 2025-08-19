using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CropTrack.Models;
using CropTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CropTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FarmerController : ControllerBase
    {
        private readonly IFarmerService _farmerService;
        private readonly IConfiguration _config;

        public FarmerController(IFarmerService service, IConfiguration config)
        {
            _farmerService = service;
            _config = config;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterFarmerRequest request)
        {
            try
            {
                var farmer = await _farmerService.RegisterAndReturnFarmer(request);
                if (farmer == null)
                    return BadRequest("Farmer could not be registered.");

                var token = CreateToken(farmer);

                return Ok(new
                {
                    Message = "Registration successful",
                    Token = token,
                    Farmer = new
                    {
                        farmer.FarmerId,
                        farmer.Name,
                        farmer.Email
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginFarmerRequest request)
        {
            try
            {
                var farmer = await _farmerService.Login(request);
                var token = CreateToken(farmer);

                return Ok(new
                {
                    Message = "Login successful",
                    Token = token,
                    Farmer = new
                    {
                        farmer.FarmerId,
                        farmer.Name,
                        farmer.Email
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        [Authorize]
        public IActionResult RefreshToken()
        {
            var farmerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(farmerId))
                return Unauthorized();

            var farmer = new Farmer
            {
                FarmerId = int.Parse(farmerId),
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Name = User.FindFirst(ClaimTypes.Name)?.Value
            };

            var newToken = CreateToken(farmer);
            return Ok(new { Token = newToken });
        }

        private string CreateToken(Farmer farmer)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, farmer.FarmerId.ToString()),
                new Claim(ClaimTypes.Email, farmer.Email),
                new Claim(ClaimTypes.Name, farmer.Name)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}