using croptrack.Services;
using Microsoft.AspNetCore.Mvc;

namespace croptrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }


    }
}
