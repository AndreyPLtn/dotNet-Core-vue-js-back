using AccountingApp.Data;
using AccountingApp.DTOs;
using AccountingApp.Interfaces;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AccountingApp.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionService> _logger;
        private readonly IUserService _userService;

        public TransactionService(ApplicationDbContext context, ILogger<TransactionService> logger, IUserService userService)
        {
            _context = context;
            _logger = logger;
            _userService = userService;
        }

        public async Task<(bool Success, string Message, object? Data)> TransactionMoneyAsync(int fromAccountId, int toAccountId, decimal amount, ClaimsPrincipal user)
        {
            var dbUser = await _userService.GetUserAsync(user);
            if (dbUser == null)
            {
                return (false, "Пользователь не аутентифицирован", null);
            }

            var fromAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == fromAccountId && a.UserId == dbUser.Id);
            if (fromAccount == null)
            {
                return (false, "Счет не существует или не принадлежит пользователю", null);
            }

            var toAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == toAccountId);
            if (toAccount == null)
            {
                return (false, "Целевой счет не существует", null);
            }

            if (fromAccount.Balance < amount)
            {
                _logger.LogWarning("Недостаточно средств для перевода с аккаунта {FromAccountId} на аккаунт {ToAccountId} пользователя {Username}", fromAccountId, toAccountId, dbUser.Username);
                return (false, "Недостаточно средств у счета-отправителя", null);
            }

            if (fromAccount.Currency != toAccount.Currency)
            {
                return (false, "Валюты счетов различаются. Осуществите конвертацию", null);
            }

            fromAccount.Balance -= amount;
            toAccount.Balance += amount;

            var transaction = new Transaction
            {
                FromAccountId = fromAccountId,
                ToAccountId = toAccountId,
                Amount = amount,
                Date = DateTime.UtcNow,
                Currency = fromAccount.Currency
            };

            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Транзакция с аккаунта {FromAccountId} на аккаунт {ToAccountId} пользователя {Username}", fromAccountId, toAccountId, dbUser.Username);
            return (true, null, new { transaction, Accounts = await _context.Accounts.Where(a => a.UserId == dbUser.Id).Select(a => new AccountDto { Id = a.Id, Currency = a.Currency, Balance = a.Balance }).ToListAsync() });
        }
    }
}
