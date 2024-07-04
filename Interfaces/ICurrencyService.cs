using AccountingApp.DTOs;
using System.Security.Claims;

namespace AccountingApp.Interfaces
{
    public interface ICurrencyService
    {
        Task<(bool Success, string Message, object? Data)> ConvertCurrencyAsync(int accountId, string targetCurrency, ClaimsPrincipal user);
    }
}
