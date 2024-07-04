using Microsoft.AspNetCore.Mvc;
using AccountingApp.Interfaces;

namespace AccountingApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string password)
        {
            var (Success, Message, Token) = await _userService.RegisterUserAsync(username, password);
            if (!Success)
            {
                return BadRequest(Message);
            }

            return Ok(Message);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var result = await _userService.LoginUserAsync(username, password);
            if (!result.Success)
            {
                return Unauthorized(result.Message);
            }

            return Ok(new { token = result.Token });
        }
    }
}