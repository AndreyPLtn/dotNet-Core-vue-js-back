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
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) //�������� ���������� ���/������
            {
                return BadRequest("Error 400. ��������� ������ ����");
            }

            if (_context.Users.Any(u => u.Username == username)) //�������� ���������� �����
            {
                return Conflict("Error 409. ������������ � ��������� ������ ��� ����������");
            };

            var user = new Models.User() //������� ������ ������������
            {
                Username = username
            };
                      
            using (var sha256Hash = SHA256.Create()) //�������� � ������������
            {
                byte[] passwordBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                user.PasswordHash = BitConverter.ToString(passwordBytes).Replace("-", String.Empty);
            };

            _context.Users.Add(user); //��������� user
            _context.SaveChanges(); //��������� � ��

            _logger.LogInformation($"�������� ����������� ������������ {username}");
            return Ok($"������������ {username} ������� ���������������, ���: {user.PasswordHash}"); //������� ������ � �����
        }

        [HttpPost("login")]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.First(u => u.Username == username); //����� �� ����� � ��
            if (user == null) //�������� ������������� ������������
            {
                //������� ������, ���� �� ����� ������������
                return NotFound("Error 404: ������������ � ����� ������ �� ����������");
            }

            //������� ��������� ���� ������ �� �������� ������
            using (var sha256Hash = SHA256.Create())
            {
                byte[] passwordBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                string hashedPassword = BitConverter.ToString(passwordBytes).Replace("-", string.Empty);

                if (hashedPassword != user.PasswordHash)
                {
                    return Unauthorized("Error 403: �������� ������");
                }
            }

            var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
            // ������� JWT-�����
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TytNeHitrimSposobomYaKlady256Bit"));
            var jwt = new JwtSecurityToken(
                    issuer: "tgk",
                    audience: "TgkWebApp",
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(2),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            //�������� ����� � ������ ������
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return Ok(new { token });
        }
    }
}