using AplikasiWebMethodSOAP.Models;
using Microsoft.AspNetCore.Mvc;

namespace AplikasiWebMethodSOAP.Controllers;

/// <summary>
/// API Controller untuk operasi perbankan melalui SOAP service
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BankingApiController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private string ServiceUrl => _configuration["BankingServiceUrl"] ?? "http://localhost:5225/BankingService.svc";

    public BankingApiController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Membuat akun baru di sistem
    /// </summary>
    /// <param name="request">Data akun baru termasuk nama, tipe, dan saldo awal</param>
    /// <returns>Data akun yang berhasil dibuat</returns>
    /// <response code="201">Akun berhasil dibuat</response>
    /// <response code="400">Data tidak valid</response>
    [HttpPost("accounts")]
    [ProducesResponseType(typeof(Account), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var account = await client.CreateAccountAsync(request);
            return CreatedAtAction(nameof(GetAccount), new { accountNumber = account.AccountNumber }, account);
        }
        catch (Exception ex)
        {
            return BadRequest(new ServiceResponse { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Mendapatkan detail akun berdasarkan nomor akun
    /// </summary>
    /// <param name="accountNumber">Nomor akun yang akan dilihat detailnya</param>
    /// <returns>Detail akun</returns>
    /// <response code="200">Detail akun ditemukan</response>
    /// <response code="404">Akun tidak ditemukan</response>
    [HttpGet("accounts/{accountNumber}")]
    [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccount(string accountNumber)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var account = await client.GetAccountAsync(accountNumber);
            if (account == null)
            {
                return NotFound(new ServiceResponse { Success = false, Message = "Akun tidak ditemukan" });
            }
            return Ok(account);
        }
        catch (Exception ex)
        {
            return BadRequest(new ServiceResponse { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Mendapatkan semua daftar akun yang terdaftar
    /// </summary>
    /// <returns>Daftar semua akun</returns>
    /// <response code="200">Daftar semua akun</response>
    [HttpGet("accounts")]
    [ProducesResponseType(typeof(List<Account>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAccounts()
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var accounts = await client.GetAllAccountsAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return BadRequest(new ServiceResponse { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Melakukan deposit ke akun
    /// </summary>
    /// <param name="request">Data deposit termasuk nomor akun dan jumlah</param>
    /// <returns>Data akun setelah deposit</returns>
    /// <response code="200">Deposit berhasil</response>
    /// <response code="400">Data tidak valid atau akun tidak ditemukan</response>
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var account = await client.DepositAsync(request);
            return Ok(account);
        }
        catch (Exception ex)
        {
            return BadRequest(new ServiceResponse { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Melakukan penarikan dari akun
    /// </summary>
    /// <param name="request">Data penarikan termasuk nomor akun dan jumlah</param>
    /// <returns>Data akun setelah penarikan</returns>
    /// <response code="200">Penarikan berhasil</response>
    /// <response code="400">Data tidak valid, saldo tidak mencukupi, atau akun tidak ditemukan</response>
    [HttpPost("withdraw")]
    [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var account = await client.WithdrawAsync(request);
            return Ok(account);
        }
        catch (Exception ex)
        {
            return BadRequest(new ServiceResponse { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Melakukan transfer antar akun
    /// </summary>
    /// <param name="request">Data transfer termasuk akun sumber, tujuan, dan jumlah</param>
    /// <returns>Hasil transfer</returns>
    /// <response code="200">Transfer berhasil</response>
    /// <response code="400">Data tidak valid, saldo tidak mencukupi, atau akun tidak ditemukan</response>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var response = await client.TransferAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new ServiceResponse { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Mendapatkan riwayat transaksi akun
    /// </summary>
    /// <param name="accountNumber">Nomor akun yang akan dilihat riwayatnya</param>
    /// <returns>Daftar transaksi akun</returns>
    /// <response code="200">Riwayat transaksi</response>
    /// <response code="400">Terjadi kesalahan</response>
    [HttpGet("accounts/{accountNumber}/transactions")]
    [ProducesResponseType(typeof(List<Transaction>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTransactions(string accountNumber)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var transactions = await client.GetTransactionHistoryAsync(accountNumber);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            return BadRequest(new ServiceResponse { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Memeriksa saldo akun
    /// </summary>
    /// <param name="accountNumber">Nomor akun yang akan diperiksa saldonya</param>
    /// <returns>Saldo akun</returns>
    /// <response code="200">Saldo akun</response>
    /// <response code="404">Akun tidak ditemukan</response>
    [HttpGet("accounts/{accountNumber}/balance")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance(string accountNumber)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var balance = await client.GetBalanceAsync(accountNumber);
            return Ok(new { AccountNumber = accountNumber, Balance = balance });
        }
        catch (Exception ex)
        {
            return NotFound(new ServiceResponse { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Menutup akun (saldo harus 0)
    /// </summary>
    /// <param name="accountNumber">Nomor akun yang akan ditutup</param>
    /// <returns>Hasil penutupan akun</returns>
    /// <response code="200">Akun berhasil ditutup</response>
    /// <response code="400">Akun masih memiliki saldo atau tidak ditemukan</response>
    [HttpDelete("accounts/{accountNumber}")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CloseAccount(string accountNumber)
    {
        try
        {
            using var client = new BankingClient(ServiceUrl);
            var response = await client.CloseAccountAsync(accountNumber);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new ServiceResponse { Success = false, Message = ex.Message });
        }
    }
}
