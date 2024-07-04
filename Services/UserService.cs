using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;
using AccountingApp.Interfaces;

namespace AccountingApp.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, string? Token)> RegisterUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Не указаны имя пользователя или пароль", null);
            }

            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                _logger.LogWarning("Попытка регистрации с уже существующим именем пользователя: {Username}", username);
                return (false, "Пользователь с таким именем уже существует", null);
            }

            var user = new User
            {
                Username = username,
                PasswordHash = ComputeHash(password)
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Успешная регистрация пользователя {Username}", username);
            return (true, $"Пользователь {username} успешно зарегистрирован, Хеш: {user.PasswordHash}", null);
        }

        public async Task<(bool Success, string Message, string? Token)> LoginUserAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                _logger.LogWarning("Попытка входа с несуществующим именем пользователя: {Username}", username);
                return (false, "Пользователя с таким именем не существует", null);
            }

            if (ComputeHash(password) != user.PasswordHash)
            {
                _logger.LogWarning("Попытка входа с неверным паролем для пользователя: {Username}", username);
                return (false, "Неверный пароль", null);
            }

            var token = GenerateJwtToken(username);
            return (true, null, token);
        }

        public async Task<User?> GetUserAsync(ClaimsPrincipal user)
        {
            if (user.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            {
                return null;
            }
            var username = identity.Name;
            return await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        }

        private string ComputeHash(string input)
        {
            return BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(input))).Replace("-", string.Empty);
        }

        private string GenerateJwtToken(string username)
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, username) };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TytNeHitrimSposobomYaKlady256Bit"));
            var jwt = new JwtSecurityToken(
                    issuer: "tgk",
                    audience: "TgkWebApp",
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(2),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
