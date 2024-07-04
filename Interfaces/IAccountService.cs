using AccountingApp.DTOs;
using AccountingApp.Models;
using System.Security.Claims;

namespace AccountingApp.Interfaces
{
    public interface IAccountService
    {
        Task<Account?> GetAccountAsync(int accountId, int userId);
        Task<List<AccountDto>> GetAccountsListAsync(int userId);
        Task<(bool Success, string Message, object? Data)> CreateAccountAsync(string currency, ClaimsPrincipal user);
        Task<User?> GetUserAsync(ClaimsPrincipal user);
    }
}
