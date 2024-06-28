using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AccountingApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HashSet<string> _supportedCurrencies = ["RUB", "MNT"]; // MNT - код тугриков

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        private static ClaimsIdentity? GetIdentity(HttpContext httpContext)
        {
            return httpContext.User.Identity as ClaimsIdentity;
        }

        private IActionResult ValidateUser(out User user)
        {
            user = null!;
            var identity = GetIdentity(HttpContext);
            if (identity == null || !identity.IsAuthenticated)
            {
                return Unauthorized("Пользователь не аутентифицирован");
            }

            var username = identity.Name;
            user = _context.Users.First(u => u.Username == username);
            if (user == null)
            {
                return NotFound("Пользователя не существует");
            }
            return Ok();
        }
        private IActionResult ValidateAccount(int accountId, User user, out Account account)
        {
            account = _context.Accounts.First(a => a.Id == accountId && a.UserId == user.Id);
            if (account == null)
            {
                return NotFound("Счет не существует или не принадлежит пользователю");
            }
            return Ok();
        }

        private List<Account> GetAccounts(int userId)
        {
            return _context.Accounts.Where(a => a.UserId == userId).ToList();
        }

        [Authorize]
        [HttpPost("account/create")]
        public IActionResult CreateAccount(string currency)
        {
            if (string.IsNullOrEmpty(currency))
            {
                return BadRequest("Валюта не введена");
            }

            if (!_supportedCurrencies.Contains(currency.ToUpper()))
            {
                return BadRequest("Валюта введена неверно");
            }

            var validationResult = ValidateUser(out var user);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            var accountCounter = _context.Accounts.Count(a => a.UserId == user.Id);
            if (accountCounter >= 5)
            {
                return BadRequest("Не больше 5 аккаунтов");
            }

            var newAccount = new Account
            {
                UserId = user.Id,
                Currency = currency.ToUpper(),
                Balance = 666666
            };

            _context.Accounts.Add(newAccount);
            _context.SaveChanges();

            return Ok(new { user.Username, AccountId = newAccount.Id, Currency = currency, Accounts = GetAccounts(user.Id) });
        }

        [Authorize]
        [HttpPost("account/convert")]
        public IActionResult ConvertCurrency(int accountId, string targetCurrency)
        {
            var validationResult = ValidateUser(out var user);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            validationResult = ValidateAccount(accountId, user, out var account);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            if (string.IsNullOrEmpty(targetCurrency) || !_supportedCurrencies.Contains(targetCurrency.ToUpper()))
            {
                return BadRequest("Некорректная целевая валюта");
            }

            if (account.Currency == targetCurrency.ToUpper())
            {
                return BadRequest("Целевая валюта совпадает с конвертируемой валютой");
            }

            var currencyRate = _context.CurrencyRates.FirstOrDefault(
                                cr => cr.FromCurrency == account.Currency &&
                                cr.ToCurrency == targetCurrency);

            if (currencyRate == null)
            {
                return BadRequest("Курс конвертации для данной валюты не найден");
            }

            account.Balance *= currencyRate.Rate;
            account.Currency = targetCurrency.ToUpper();

            _context.SaveChanges();

            return Ok(new { AccountId = account.Id, NewCurrency = account.Currency, NewBalance = account.Balance, Accounts = GetAccounts(user.Id) });
        }

        [Authorize]
        [HttpPost("account/transaction")]
        public IActionResult TransactionMoney(int fromAccountId, int toAccountId, decimal amount)
        {
            var validationResult = ValidateUser(out var user);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            validationResult = ValidateAccount(fromAccountId, user, out var fromAccount);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            var toAccount = _context.Accounts.First(a => a.Id == toAccountId);
            if (toAccount == null)
            {
                return NotFound("Целевой счет не существует");
            }

            if (fromAccount.Balance < amount)
            {
                return BadRequest("Недостаточно средству у счета-отправителя");
            }

            if(fromAccount.Currency != toAccount.Currency)
            {
                return BadRequest("Валюты счетов различаются. Осуществите конвертацию");
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

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            return Ok(new { transaction, Accounts = GetAccounts(user.Id) });
        }

        [Authorize]
        [HttpGet("account/getAccounts")]
        public IActionResult GetAccounts()
        {
            var validationResult = ValidateUser(out var user);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            var accounts = _context.Accounts
                .Where(a => a.UserId == user.Id)
                .OrderBy(a => a.Id)
                .Select(a => new
                {
                    a.Id,
                    a.Currency,
                    a.Balance
                })
                .ToList();

            return Ok(accounts);
        }
    }
}