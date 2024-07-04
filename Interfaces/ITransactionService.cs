using AccountingApp.DTOs;
using System.Security.Claims;

namespace AccountingApp.Interfaces
{
    public interface ITransactionService
    {
        Task<(bool Success, string Message, object? Data)> TransactionMoneyAsync(int fromAccountId, int toAccountId, decimal amount, ClaimsPrincipal user);
    }
}
