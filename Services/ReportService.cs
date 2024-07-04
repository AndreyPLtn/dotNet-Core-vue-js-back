using AccountingApp.Data;
using AccountingApp.DTOs;
using AccountingApp.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AccountingApp.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IReportService> _logger;

        public ReportService(ApplicationDbContext context, ILogger<IReportService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<(bool Success, string Message, List<TransactionDto> Data)> GenerateReportAsync(
            ClaimsPrincipal user, DateTime? startDate, DateTime? endDate, string? currency, int? fromAccId, int? toAccId)
        {
            if (user.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            {
                return (false, "Пользователь не аутентифицирован", new List<TransactionDto>());
            }

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return (false, "Неверно указана дата начала/окончания", new List<TransactionDto>());
            }

            var username = identity.Name;

            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (dbUser == null)
            {
                return (false, "Пользователь не найден", new List<TransactionDto>());
            }

            var query = _context.Transactions.AsQueryable();

            var userAccountIds = await _context.Accounts
                .Where(a => a.UserId == dbUser.Id)
                .Select(a => a.Id)
                .ToListAsync();

            query = query.Where(t => userAccountIds.Contains(t.FromAccountId) || userAccountIds.Contains(t.ToAccountId));

            if (fromAccId.HasValue)
            {
                query = query.Where(t => t.FromAccountId == fromAccId.Value);
            }

            if (toAccId.HasValue)
            {
                query = query.Where(t => t.ToAccountId == toAccId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Date <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(currency))
            {
                currency = currency.ToUpper();
                if (currency == "RUB" || currency == "MNT")
                {
                    query = query.Where(t => t.Currency.ToUpper() == currency);
                }
                else
                {
                    return (false, "Валюта указана неверно", new List<TransactionDto>());
                }
            }

            var transactions = await query.ToListAsync();

            if (transactions.Count == 0)
            {
                return (true, "Транзакций нет за указанный период", new List<TransactionDto>());
            }

            var reportData = transactions.Select(t => new TransactionDto
            {
                Date = t.Date,
                Currency = t.Currency,
                Amount = t.Amount,
                Id = t.Id,
                FromAccountId = t.FromAccountId,
                ToAccountId = t.ToAccountId
            }).ToList();

            _logger.LogInformation("Отчет для {Username} успешно создан", username);
            return (true, "Отчет успешно создан", reportData);
        }
    }
}