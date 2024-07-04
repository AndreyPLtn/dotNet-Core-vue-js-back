using AccountingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("transaction")]
        public async Task<IActionResult> TransactionMoney(int fromAccountId, int toAccountId, decimal amount)
        {
            var result = await _transactionService.TransactionMoneyAsync(fromAccountId, toAccountId, amount, HttpContext.User);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }
    }
}
