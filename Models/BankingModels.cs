using System.Runtime.Serialization;

namespace AplikasiWebMethodSOAP.Models;

[DataContract]
public class Account
{
    [DataMember]
    public string AccountNumber { get; set; } = string.Empty;

    [DataMember]
    public string AccountHolderName { get; set; } = string.Empty;

    [DataMember]
    public decimal Balance { get; set; }

    [DataMember]
    public string AccountType { get; set; } = string.Empty;

    [DataMember]
    public DateTime CreatedDate { get; set; }
}

[DataContract]
public class Transaction
{
    [DataMember]
    public string TransactionId { get; set; } = string.Empty;

    [DataMember]
    public string FromAccountNumber { get; set; } = string.Empty;

    [DataMember]
    public string ToAccountNumber { get; set; } = string.Empty;

    [DataMember]
    public decimal Amount { get; set; }

    [DataMember]
    public string TransactionType { get; set; } = string.Empty;

    [DataMember]
    public DateTime TransactionDate { get; set; }

    [DataMember]
    public string Description { get; set; } = string.Empty;
}

[DataContract]
public class CreateAccountRequest
{
    [DataMember]
    public string AccountHolderName { get; set; } = string.Empty;

    [DataMember]
    public decimal InitialBalance { get; set; }

    [DataMember]
    public string AccountType { get; set; } = string.Empty;
}

[DataContract]
public class DepositRequest
{
    [DataMember]
    public string AccountNumber { get; set; } = string.Empty;

    [DataMember]
    public decimal Amount { get; set; }
}

[DataContract]
public class WithdrawRequest
{
    [DataMember]
    public string AccountNumber { get; set; } = string.Empty;

    [DataMember]
    public decimal Amount { get; set; }
}

[DataContract]
public class TransferRequest
{
    [DataMember]
    public string FromAccountNumber { get; set; } = string.Empty;

    [DataMember]
    public string ToAccountNumber { get; set; } = string.Empty;

    [DataMember]
    public decimal Amount { get; set; }
}

[DataContract]
public class ServiceResponse
{
    [DataMember]
    public bool Success { get; set; }

    [DataMember]
    public string Message { get; set; } = string.Empty;
}
