using AccountingApp.Data;
using AccountingApp.DTOs;
using AccountingApp.Interfaces;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AccountingApp.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IUserService _userService;
        private readonly HashSet<string> _supportedCurrencies = new() { "RUB", "MNT" };

        public CurrencyService(ApplicationDbContext context, ILogger<CurrencyService> logger, IUserService userService)
        {
            _context = context;
            _logger = logger;
            _userService = userService;
        }

        public async Task<(bool Success, string Message, object? Data)> ConvertCurrencyAsync(int accountId, string targetCurrency, ClaimsPrincipal user)
        {
            var dbUser = await _userService.GetUserAsync(user);
            if (dbUser == null)
            {
                return (false, "Пользователь не аутентифицирован", null);
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == dbUser.Id);
            if (account == null)
            {
                return (false, "Счет не существует или не принадлежит пользователю", null);
            }

            if (!IsValidCurrency(targetCurrency) || account.Currency == targetCurrency.ToUpper())
            {
                return (false, "Некорректная целевая валюта или совпадает с текущей", null);
            }

            var currencyRate = await _context.CurrencyRates.FirstOrDefaultAsync(
                                cr => cr.FromCurrency == account.Currency &&
                                cr.ToCurrency == targetCurrency);

            if (currencyRate == null)
            {
                _logger.LogWarning("Курс конвертации не найден для валюты {FromCurrency} в {ToCurrency}", account.Currency, targetCurrency);
                return (false, "Курс конвертации для данной валюты не найден", null);
            }

            account.Balance *= currencyRate.Rate;
            account.Currency = targetCurrency.ToUpper();

            await _context.SaveChangesAsync();

            _logger.LogInformation("Успешная конвертация для аккаунта {AccountId} пользователя {Username} в {NewCurrency}", account.Id, dbUser.Username, account.Currency);
            return (true, null, new { AccountId = account.Id, NewCurrency = account.Currency, NewBalance = account.Balance, Accounts = await _context.Accounts.Where(a => a.UserId == dbUser.Id).Select(a => new AccountDto { Id = a.Id, Currency = a.Currency, Balance = a.Balance }).ToListAsync() });
        }

        private bool IsValidCurrency(string currency)
        {
            return !string.IsNullOrEmpty(currency) && _supportedCurrencies.Contains(currency.ToUpper());
        }
    }
}
