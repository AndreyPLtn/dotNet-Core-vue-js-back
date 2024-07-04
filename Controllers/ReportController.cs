using AccountingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("report")]
        public async Task<IActionResult> GetReport(
            DateTime? startDate, DateTime? endDate, string? currency = null, int? fromAccId = null, int? toAccId = null)
        {
            var result = await _reportService.GenerateReportAsync(HttpContext.User, startDate, endDate, currency, fromAccId, toAccId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            _logger.LogInformation("Отчет успешно создан");
            return Ok(result.Data);
        }
    }
}