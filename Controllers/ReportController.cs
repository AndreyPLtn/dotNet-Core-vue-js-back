using AccountingApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AccountingApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("report")]
        public IActionResult GetReport(
            DateTime? startDate, DateTime? endDate, string? currency = null, int? fromAccId = null, int? toAccId = null)
        {          
            if (HttpContext.User.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            {
                return Unauthorized("Пользователь не аутентифицирован");
            }

            startDate = startDate.HasValue ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc) : (DateTime?)null;
            endDate = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc) : (DateTime?)null;

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest("Неверно указана дата начала/окончания");
            }

            var username = identity.Name;

            var user = _context.Users.First(u => u.Username == username);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }
            
            var query = _context.Transactions.AsQueryable();
            
            var userAccountIds = _context.Accounts
                .Where(a => a.UserId == user.Id)
                .Select(a => a.Id)
                .ToList();

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
                    return BadRequest("Неверно указана валюта");
                }
            }

            var transactions = query.ToList();

            if (transactions.Count == 0)
            {
                return Ok(new { message = "Транзакций нет за указанный период" });
            }

            var reportData = transactions.Select(t => new
            {
                t.Date,
                t.Currency,
                t.Amount,
                t.Id,
                t.FromAccountId,
                t.ToAccountId
            }).ToList();

            return Ok(reportData);
        }
    }
}