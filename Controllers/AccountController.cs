using AccountingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateAccount(string currency)
        {
            var result = await _accountService.CreateAccountAsync(currency, HttpContext.User);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("getAccounts")]
        public async Task<IActionResult> GetAccountsAsync()
        {
            var user = await _accountService.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return Unauthorized("Пользователь не аутентифицирован");
            }

            var accounts = await _accountService.GetAccountsListAsync(user.Id);

            return Ok(accounts);
        }
    }
}
