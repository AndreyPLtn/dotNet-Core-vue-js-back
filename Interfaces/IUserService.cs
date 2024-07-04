using AccountingApp.DTOs;
using AccountingApp.Models;
using System.Security.Claims;

namespace AccountingApp.Interfaces
{
    public interface IUserService
    {
        Task<(bool Success, string Message, string Token)> RegisterUserAsync(string username, string password);
        Task<(bool Success, string Message, string Token)> LoginUserAsync(string username, string password);
        Task<User?> GetUserAsync(ClaimsPrincipal user);
    }
}
