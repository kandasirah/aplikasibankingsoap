using AplikasiWebMethodSOAP.Contracts;
using AplikasiWebMethodSOAP.Models;
using CoreWCF;

namespace AplikasiWebMethodSOAP.Services;

public class BankingService : IBankingService
{
    private static readonly List<Account> _accounts = new();
    private static readonly List<Transaction> _transactions = new();
    private static int _transactionCounter = 0;

    public Task<Account> CreateAccountAsync(CreateAccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccountHolderName))
            throw new FaultException("Nama pemegang akun tidak boleh kosong");

        if (request.InitialBalance < 0)
            throw new FaultException("Saldo awal tidak boleh negatif");

        if (string.IsNullOrWhiteSpace(request.AccountType))
            throw new FaultException("Tipe akun tidak boleh kosong");

        var account = new Account
        {
            AccountNumber = GenerateAccountNumber(),
            AccountHolderName = request.AccountHolderName,
            Balance = request.InitialBalance,
            AccountType = request.AccountType,
            CreatedDate = DateTime.UtcNow
        };

        _accounts.Add(account);

        if (request.InitialBalance > 0)
        {
            var transaction = new Transaction
            {
                TransactionId = GenerateTransactionId(),
                FromAccountNumber = "SYSTEM",
                ToAccountNumber = account.AccountNumber,
                Amount = request.InitialBalance,
                TransactionType = "DEPOSIT",
                TransactionDate = DateTime.UtcNow,
                Description = "Setoran awal pembukaan akun"
            };
            _transactions.Add(transaction);
        }

        return Task.FromResult(account);
    }

    public Task<Account?> GetAccountAsync(string accountNumber)
    {
        var account = _accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
        return Task.FromResult(account);
    }

    public Task<List<Account>> GetAllAccountsAsync()
    {
        return Task.FromResult(_accounts.ToList());
    }

    public Task<Account> DepositAsync(DepositRequest request)
    {
        if (request.Amount <= 0)
            throw new FaultException("Jumlah deposit harus lebih dari 0");

        var account = _accounts.FirstOrDefault(a => a.AccountNumber == request.AccountNumber)
            ?? throw new FaultException($"Akun dengan nomor {request.AccountNumber} tidak ditemukan");

        account.Balance += request.Amount;

        var transaction = new Transaction
        {
            TransactionId = GenerateTransactionId(),
            FromAccountNumber = "EXTERNAL",
            ToAccountNumber = account.AccountNumber,
            Amount = request.Amount,
            TransactionType = "DEPOSIT",
            TransactionDate = DateTime.UtcNow,
            Description = "Deposit ke akun"
        };
        _transactions.Add(transaction);

        return Task.FromResult(account);
    }

    public Task<Account> WithdrawAsync(WithdrawRequest request)
    {
        if (request.Amount <= 0)
            throw new FaultException("Jumlah penarikan harus lebih dari 0");

        var account = _accounts.FirstOrDefault(a => a.AccountNumber == request.AccountNumber)
            ?? throw new FaultException($"Akun dengan nomor {request.AccountNumber} tidak ditemukan");

        if (account.Balance < request.Amount)
            throw new FaultException("Saldo tidak mencukupi");

        account.Balance -= request.Amount;

        var transaction = new Transaction
        {
            TransactionId = GenerateTransactionId(),
            FromAccountNumber = account.AccountNumber,
            ToAccountNumber = "EXTERNAL",
            Amount = request.Amount,
            TransactionType = "WITHDRAWAL",
            TransactionDate = DateTime.UtcNow,
            Description = "Penarikan dari akun"
        };
        _transactions.Add(transaction);

        return Task.FromResult(account);
    }

    public Task<ServiceResponse> TransferAsync(TransferRequest request)
    {
        if (request.Amount <= 0)
            throw new FaultException("Jumlah transfer harus lebih dari 0");

        if (request.FromAccountNumber == request.ToAccountNumber)
            throw new FaultException("Akun sumber dan tujuan tidak boleh sama");

        var fromAccount = _accounts.FirstOrDefault(a => a.AccountNumber == request.FromAccountNumber)
            ?? throw new FaultException($"Akun sumber dengan nomor {request.FromAccountNumber} tidak ditemukan");

        var toAccount = _accounts.FirstOrDefault(a => a.AccountNumber == request.ToAccountNumber)
            ?? throw new FaultException($"Akun tujuan dengan nomor {request.ToAccountNumber} tidak ditemukan");

        if (fromAccount.Balance < request.Amount)
            throw new FaultException("Saldo tidak mencukupi untuk transfer");

        fromAccount.Balance -= request.Amount;
        toAccount.Balance += request.Amount;

        var transaction = new Transaction
        {
            TransactionId = GenerateTransactionId(),
            FromAccountNumber = fromAccount.AccountNumber,
            ToAccountNumber = toAccount.AccountNumber,
            Amount = request.Amount,
            TransactionType = "TRANSFER",
            TransactionDate = DateTime.UtcNow,
            Description = $"Transfer dari {fromAccount.AccountNumber} ke {toAccount.AccountNumber}"
        };
        _transactions.Add(transaction);

        return Task.FromResult(new ServiceResponse
        {
            Success = true,
            Message = $"Transfer sebesar {request.Amount:C} berhasil dari {fromAccount.AccountNumber} ke {toAccount.AccountNumber}"
        });
    }

    public Task<List<Transaction>> GetTransactionHistoryAsync(string accountNumber)
    {
        var transactions = _transactions
            .Where(t => t.FromAccountNumber == accountNumber || t.ToAccountNumber == accountNumber)
            .OrderByDescending(t => t.TransactionDate)
            .ToList();

        return Task.FromResult(transactions);
    }

    public Task<decimal> GetBalanceAsync(string accountNumber)
    {
        var account = _accounts.FirstOrDefault(a => a.AccountNumber == accountNumber)
            ?? throw new FaultException($"Akun dengan nomor {accountNumber} tidak ditemukan");

        return Task.FromResult(account.Balance);
    }

    public Task<ServiceResponse> CloseAccountAsync(string accountNumber)
    {
        var account = _accounts.FirstOrDefault(a => a.AccountNumber == accountNumber)
            ?? throw new FaultException($"Akun dengan nomor {accountNumber} tidak ditemukan");

        if (account.Balance > 0)
            throw new FaultException("Akun tidak dapat ditutup karena masih memiliki saldo. Silakan tarik saldo terlebih dahulu.");

        _accounts.Remove(account);

        return Task.FromResult(new ServiceResponse
        {
            Success = true,
            Message = $"Akun {accountNumber} berhasil ditutup"
        });
    }

    private static string GenerateAccountNumber()
    {
        return $"ACC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }

    private static string GenerateTransactionId()
    {
        _transactionCounter++;
        return $"TRX-{DateTime.UtcNow:yyyyMMddHHmmss}-{_transactionCounter:D6}";
    }
}
