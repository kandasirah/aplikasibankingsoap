using AplikasiWebMethodSOAP.Models;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AplikasiWebMethodSOAP.Controllers;

public class BankingClient : IDisposable
{
    private readonly string _serviceUrl;
    private readonly HttpClient _httpClient;
    private const string DataNamespace = "http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models";

    public BankingClient(string serviceUrl)
    {
        _serviceUrl = serviceUrl.TrimEnd('/');
        _httpClient = new HttpClient();
    }

    private async Task<XDocument> CallServiceAsync(string operation, string requestContent)
    {
        var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <{operation} xmlns=""http://tempuri.org/"">
      {requestContent}
    </{operation}>
  </soap:Body>
</soap:Envelope>";

        var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", $"\"http://tempuri.org/IBankingService/{operation}\"");

        var response = await _httpClient.PostAsync(_serviceUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Service error: {responseContent}");
        }

        return XDocument.Parse(responseContent);
    }

    private XNamespace ns => "http://tempuri.org/";
    private XNamespace dataNs => DataNamespace;

    public async Task<Account> CreateAccountAsync(CreateAccountRequest request)
    {
        var requestContent = $@"<request xmlns:q1=""{DataNamespace}"">
        <q1:AccountHolderName>{EscapeXml(request.AccountHolderName)}</q1:AccountHolderName>
        <q1:AccountType>{EscapeXml(request.AccountType)}</q1:AccountType>
        <q1:InitialBalance>{request.InitialBalance}</q1:InitialBalance>
      </request>";

        var doc = await CallServiceAsync("CreateAccount", requestContent);
        var result = doc.Descendants(ns + "CreateAccountResult").FirstOrDefault();
        return ParseAccountFromElement(result);
    }

    public async Task<Account?> GetAccountAsync(string accountNumber)
    {
        var requestContent = $"<accountNumber>{EscapeXml(accountNumber)}</accountNumber>";
        var doc = await CallServiceAsync("GetAccount", requestContent);
        var result = doc.Descendants(ns + "GetAccountResult").FirstOrDefault();
        if (result == null || string.IsNullOrEmpty(result.Value) && !result.HasElements)
            return null;
        return ParseAccountFromElement(result);
    }

    public async Task<List<Account>> GetAllAccountsAsync()
    {
        var doc = await CallServiceAsync("GetAllAccounts", "");
        var accounts = new List<Account>();
        var resultElement = doc.Descendants(ns + "GetAllAccountsResult").FirstOrDefault();
        if (resultElement != null)
        {
            foreach (var accountElement in resultElement.Elements(dataNs + "Account"))
            {
                accounts.Add(ParseAccountFromElement(accountElement));
            }
        }
        return accounts;
    }

    public async Task<Account> DepositAsync(DepositRequest request)
    {
        var requestContent = $@"<request xmlns:q1=""{DataNamespace}"">
        <q1:AccountNumber>{EscapeXml(request.AccountNumber)}</q1:AccountNumber>
        <q1:Amount>{request.Amount}</q1:Amount>
      </request>";

        var doc = await CallServiceAsync("Deposit", requestContent);
        var result = doc.Descendants(ns + "DepositResult").FirstOrDefault();
        return ParseAccountFromElement(result);
    }

    public async Task<Account> WithdrawAsync(WithdrawRequest request)
    {
        var requestContent = $@"<request xmlns:q1=""{DataNamespace}"">
        <q1:AccountNumber>{EscapeXml(request.AccountNumber)}</q1:AccountNumber>
        <q1:Amount>{request.Amount}</q1:Amount>
      </request>";

        var doc = await CallServiceAsync("Withdraw", requestContent);
        var result = doc.Descendants(ns + "WithdrawResult").FirstOrDefault();
        return ParseAccountFromElement(result);
    }

    public async Task<ServiceResponse> TransferAsync(TransferRequest request)
    {
        var requestContent = $@"<request xmlns:q1=""{DataNamespace}"">
        <q1:Amount>{request.Amount}</q1:Amount>
        <q1:FromAccountNumber>{EscapeXml(request.FromAccountNumber)}</q1:FromAccountNumber>
        <q1:ToAccountNumber>{EscapeXml(request.ToAccountNumber)}</q1:ToAccountNumber>
      </request>";

        var doc = await CallServiceAsync("Transfer", requestContent);
        var result = doc.Descendants(ns + "TransferResult").FirstOrDefault();
        return ParseServiceResponse(result);
    }

    public async Task<List<Transaction>> GetTransactionHistoryAsync(string accountNumber)
    {
        var requestContent = $"<accountNumber>{EscapeXml(accountNumber)}</accountNumber>";
        var doc = await CallServiceAsync("GetTransactionHistory", requestContent);
        var transactions = new List<Transaction>();
        var resultElement = doc.Descendants(ns + "GetTransactionHistoryResult").FirstOrDefault();
        if (resultElement != null)
        {
            foreach (var trxElement in resultElement.Elements(dataNs + "Transaction"))
            {
                transactions.Add(ParseTransaction(trxElement));
            }
        }
        return transactions;
    }

    public async Task<decimal> GetBalanceAsync(string accountNumber)
    {
        var requestContent = $"<accountNumber>{EscapeXml(accountNumber)}</accountNumber>";
        var doc = await CallServiceAsync("GetBalance", requestContent);
        var result = doc.Descendants(ns + "GetBalanceResult").FirstOrDefault();
        return result != null && decimal.TryParse(result.Value, out var balance) ? balance : 0;
    }

    public async Task<ServiceResponse> CloseAccountAsync(string accountNumber)
    {
        var requestContent = $"<accountNumber>{EscapeXml(accountNumber)}</accountNumber>";
        var doc = await CallServiceAsync("CloseAccount", requestContent);
        var result = doc.Descendants(ns + "CloseAccountResult").FirstOrDefault();
        return ParseServiceResponse(result);
    }

    private Account ParseAccountFromElement(XElement? element)
    {
        if (element == null) return new Account();

        return new Account
        {
            AccountHolderName = element.Element(dataNs + "AccountHolderName")?.Value ?? "",
            AccountNumber = element.Element(dataNs + "AccountNumber")?.Value ?? "",
            AccountType = element.Element(dataNs + "AccountType")?.Value ?? "",
            Balance = decimal.TryParse(element.Element(dataNs + "Balance")?.Value, out var balance) ? balance : 0,
            CreatedDate = DateTime.TryParse(element.Element(dataNs + "CreatedDate")?.Value, out var date) ? date : DateTime.MinValue
        };
    }

    private ServiceResponse ParseServiceResponse(XElement? element)
    {
        if (element == null) return new ServiceResponse();

        return new ServiceResponse
        {
            Success = bool.TryParse(element.Element(dataNs + "Success")?.Value, out var success) && success,
            Message = element.Element(dataNs + "Message")?.Value ?? ""
        };
    }

    private Transaction ParseTransaction(XElement element)
    {
        return new Transaction
        {
            TransactionId = element.Element(dataNs + "TransactionId")?.Value ?? "",
            FromAccountNumber = element.Element(dataNs + "FromAccountNumber")?.Value ?? "",
            ToAccountNumber = element.Element(dataNs + "ToAccountNumber")?.Value ?? "",
            Amount = decimal.TryParse(element.Element(dataNs + "Amount")?.Value, out var amount) ? amount : 0,
            TransactionType = element.Element(dataNs + "TransactionType")?.Value ?? "",
            TransactionDate = DateTime.TryParse(element.Element(dataNs + "TransactionDate")?.Value, out var date) ? date : DateTime.MinValue,
            Description = element.Element(dataNs + "Description")?.Value ?? ""
        };
    }

    private static string EscapeXml(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
