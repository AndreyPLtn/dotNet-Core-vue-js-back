using AccountingApp.DTOs;
using System.Security.Claims;

namespace AccountingApp.Interfaces
{
    public interface IReportService
    {
        Task<(bool Success, string Message, List<TransactionDto> Data)> GenerateReportAsync(
            ClaimsPrincipal user, DateTime? startDate, DateTime? endDate, string? currency, int? fromAccId, int? toAccId);
    }
}
