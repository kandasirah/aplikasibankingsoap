using AplikasiWebMethodSOAP.Models;
using CoreWCF;

namespace AplikasiWebMethodSOAP.Contracts;

[ServiceContract]
public interface IBankingService
{
    [OperationContract]
    Task<Account> CreateAccountAsync(CreateAccountRequest request);

    [OperationContract]
    Task<Account?> GetAccountAsync(string accountNumber);

    [OperationContract]
    Task<List<Account>> GetAllAccountsAsync();

    [OperationContract]
    Task<Account> DepositAsync(DepositRequest request);

    [OperationContract]
    Task<Account> WithdrawAsync(WithdrawRequest request);

    [OperationContract]
    Task<ServiceResponse> TransferAsync(TransferRequest request);

    [OperationContract]
    Task<List<Transaction>> GetTransactionHistoryAsync(string accountNumber);

    [OperationContract]
    Task<decimal> GetBalanceAsync(string accountNumber);

    [OperationContract]
    Task<ServiceResponse> CloseAccountAsync(string accountNumber);
}
