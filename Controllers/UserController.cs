using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using AccountingApp.Data;

namespace AccountingApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly ApplicationDbContext _context;

        public UserController(ILogger<UserController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("register")]
        public IActionResult Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest("Не указаны имя пользователя или пароль");
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                _logger.LogWarning("Попытка регистрации с уже существующим именем пользователя: {Username}", username);
                return Conflict("Имя пользователя занято");
            };

            var user = new Models.User()
            {
                Username = username
            };

            byte[] passwordBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            user.PasswordHash = BitConverter.ToString(passwordBytes).Replace("-", string.Empty); 

            _context.Users.Add(user);
            _context.SaveChanges();

            _logger.LogInformation("Успешная регистрация пользователя {Username}", username);
            return Ok($"Пользователь {username} успешно зарегистрирован, Хеш: {user.PasswordHash}");
        }

        [HttpPost("login")]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.First(u => u.Username == username);
            if (user == null)
            {
                _logger.LogWarning("Попытка входа с несуществующим именем пользователя: {Username}", username);
                return NotFound("Пользователя с таким именем не существует");
            }

            byte[] passwordBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            string hashedPassword = BitConverter.ToString(passwordBytes).Replace("-", string.Empty);

            if (hashedPassword != user.PasswordHash)
            {
                _logger.LogWarning("Попытка входа с неверным паролем для пользователя: {Username}", username);
                return Unauthorized("Неверный пароль");
            }

            var claims = new List<Claim> { new(ClaimTypes.Name, username) };
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TytNeHitrimSposobomYaKlady256Bit"));
            var jwt = new JwtSecurityToken(
                    issuer: "tgk",
                    audience: "TgkWebApp",
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(2),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return Ok(new { token });
        }
    }
}