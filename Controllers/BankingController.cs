using AplikasiWebMethodSOAP.Models;
using Microsoft.AspNetCore.Mvc;

namespace AplikasiWebMethodSOAP.Controllers;

public class BankingController : Controller
{
    private readonly IConfiguration _configuration;
    private string ServiceUrl => _configuration["BankingServiceUrl"] ?? "http://localhost:5225/BankingService.svc";

    public BankingController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult CreateAccount()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount(CreateAccountRequest request)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var account = await client.CreateAccountAsync(request);
            TempData["Success"] = $"Akun berhasil dibuat dengan nomor: {account.AccountNumber}";
            TempData["AccountNumber"] = account.AccountNumber;
            return RedirectToAction(nameof(AccountDetails), new { accountNumber = account.AccountNumber });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> AccountDetails(string accountNumber)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var account = await client.GetAccountAsync(accountNumber);
            if (account == null)
            {
                TempData["Error"] = "Akun tidak ditemukan";
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> AllAccounts()
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var accounts = await client.GetAllAccountsAsync();
            return View(accounts);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(new List<Account>());
        }
    }

    [HttpGet]
    public IActionResult Deposit()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Deposit(DepositRequest request)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var account = await client.DepositAsync(request);
            TempData["Success"] = $"Deposit berhasil. Saldo baru: {account.Balance:C}";
            return RedirectToAction(nameof(AccountDetails), new { accountNumber = account.AccountNumber });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(request);
        }
    }

    [HttpGet]
    public IActionResult Withdraw()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Withdraw(WithdrawRequest request)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var account = await client.WithdrawAsync(request);
            TempData["Success"] = $"Penarikan berhasil. Saldo baru: {account.Balance:C}";
            return RedirectToAction(nameof(AccountDetails), new { accountNumber = account.AccountNumber });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(request);
        }
    }

    [HttpGet]
    public IActionResult Transfer()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Transfer(TransferRequest request)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var response = await client.TransferAsync(request);
            TempData["Success"] = response.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Transactions(string accountNumber)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var transactions = await client.GetTransactionHistoryAsync(accountNumber);
            ViewBag.AccountNumber = accountNumber;
            return View(transactions);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> CheckBalance(string accountNumber)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var balance = await client.GetBalanceAsync(accountNumber);
            TempData["Balance"] = $"Saldo akun {accountNumber}: {balance:C}";
            return RedirectToAction(nameof(AccountDetails), new { accountNumber });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult CloseAccount()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CloseAccount(string accountNumber)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var response = await client.CloseAccountAsync(accountNumber);
            TempData["Success"] = response.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View();
        }
    }
}
