using AccountingApp.Data;
using AccountingApp.DTOs;
using AccountingApp.Interfaces;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AccountingApp.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountService> _logger;
        private readonly IUserService _userService;
        private readonly HashSet<string> _supportedCurrencies = new() { "RUB", "MNT" };

        public AccountService(ApplicationDbContext context, ILogger<AccountService> logger, IUserService userService)
        {
            _context = context;
            _logger = logger;
            _userService = userService;
        }

        public async Task<User?> GetUserAsync(ClaimsPrincipal user)
        {
            return await _userService.GetUserAsync(user);
        }

        public async Task<Account?> GetAccountAsync(int accountId, int userId)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);
        }

        public async Task<List<AccountDto>> GetAccountsListAsync(int userId)
        {
            return await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => new AccountDto
                {
                    Id = a.Id,
                    Currency = a.Currency,
                    Balance = a.Balance
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message, object? Data)> CreateAccountAsync(string currency, ClaimsPrincipal user)
        {
            if (!IsValidCurrency(currency))
            {
                return (false, "Валюта не введена или введена неверно", null);
            }

            var dbUser = await _userService.GetUserAsync(user);
            if (dbUser == null)
            {
                return (false, "Пользователь не аутентифицирован", null);
            }

            if (await HasReachedAccountLimit(dbUser.Id))
            {
                _logger.LogWarning("Создание более 5 аккаунтов для пользователя {Username} невозможно", dbUser.Username);
                return (false, "Не больше 5 аккаунтов", null);
            }

            var newAccount = new Account
            {
                UserId = dbUser.Id,
                Currency = currency.ToUpper(),
                Balance = 666666,
                User = dbUser
            };

            await _context.Accounts.AddAsync(newAccount);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Аккаунт для пользователя {Username}, AccountId: {AccountId} создан", dbUser.Username, newAccount.Id);
            return (true, null, new { dbUser.Username, AccountId = newAccount.Id, Currency = currency, Accounts = await GetAccountsListAsync(dbUser.Id) });
        }

        private bool IsValidCurrency(string currency)
        {
            return !string.IsNullOrEmpty(currency) && _supportedCurrencies.Contains(currency.ToUpper());
        }

        private async Task<bool> HasReachedAccountLimit(int userId)
        {
            var accountCounter = await _context.Accounts.CountAsync(a => a.UserId == userId);
            return accountCounter >= 5;
        }
    }
}
