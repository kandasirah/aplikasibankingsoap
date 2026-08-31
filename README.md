# Banking SOAP Web Service

Aplikasi banking dengan konsep **WebMethod SOAP** menggunakan **ASP.NET Core** dan **CoreWCF**.

## Daftar Isi

- [Arsitektur](#arsitektur)
- [Teknologi](#teknologi)
- [Struktur Proyek](#struktur-proyek)
- [Instalasi](#instalasi)
- [Menjalankan Aplikasi](#menjalankan-aplikasi)
- [SOAP Service API](#soap-service-api)
  - [CreateAccount](#1-createaccount)
  - [GetAccount](#2-getaccount)
  - [GetAllAccounts](#3-getallaccounts)
  - [Deposit](#4-deposit)
  - [Withdraw](#5-withdraw)
  - [Transfer](#6-transfer)
  - [GetTransactionHistory](#7-gettransactionhistory)
  - [GetBalance](#8-getbalance)
  - [CloseAccount](#9-closeaccount)
- [UI (User Interface)](#ui-user-interface)
- [Contoh Request SOAP](#contoh-request-soap)
- [Format Response](#format-response)
- [Error Handling](#error-handling)

---

## Arsitektur

```
┌─────────────────────────────────────────────────────────────────┐
│                        Client (Browser)                         │
│                     MVC Razor Pages UI                          │
└─────────────────────────────────────────────────────────────────┘
                                │
                                │ HTTP Request
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    ASP.NET Core Web API                         │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              BankingController (MVC)                      │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                │                                │
│                                │ SOAP Client Call               │
│                                ▼                                │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                   BankingClient                           │  │
│  │              (HTTP POST + SOAP Envelope)                  │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                │
                                │ SOAP Request
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    SOAP Service Endpoint                         │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │            IBankingService (Contract)                     │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                │                                │
│                                ▼                                │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              BankingService (Implementation)              │  │
│  │         - CreateAccount, Deposit, Withdraw, etc.          │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Teknologi

| Teknologi | Versi | Deskripsi |
|-----------|-------|-----------|
| .NET | 8.0 | Framework utama |
| CoreWCF | 1.9.1 | SOAP Web Service untuk .NET Core |
| ASP.NET Core MVC | 8.0 | UI Framework |
| Bootstrap | 5.3.0 | CSS Framework |
| Font Awesome | 6.4.0 | Icons |

---

## Struktur Proyek

```
AplikasiWebMethodSOAP/
├── Contracts/
│   └── IBankingService.cs          # Service contract (Interface SOAP)
├── Models/
│   └── BankingModels.cs            # Data models (Account, Transaction, Requests)
├── Services/
│   └── BankingService.cs           # Implementasi SOAP service
├── Controllers/
│   ├── BankingController.cs        # MVC Controller
│   └── BankingClient.cs            # SOAP Client untuk memanggil service
├── Views/
│   ├── Shared/
│   │   └── _Layout.cshtml          # Layout utama
│   ├── Banking/
│   │   ├── Index.cshtml            # Halaman utama
│   │   ├── CreateAccount.cshtml    # Form buat akun
│   │   ├── AccountDetails.cshtml   # Detail akun
│   │   ├── AllAccounts.cshtml      # Daftar semua akun
│   │   ├── Deposit.cshtml          # Form setor
│   │   ├── Withdraw.cshtml         # Form tarik
│   │   ├── Transfer.cshtml         # Form transfer
│   │   ├── Transactions.cshtml     # Riwayat transaksi
│   │   └── CloseAccount.cshtml     # Form tutup akun
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── wwwroot/
│   └── css/                        # Static files
├── Program.cs                      # Konfigurasi aplikasi
├── appsettings.json                # Konfigurasi
└── AplikasiWebMethodSOAP.csproj    # Project file
```

---

## Instalasi

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- IDE: Visual Studio 2022, VS Code, atau Rider

### Clone & Build

```bash
# Clone repository
git clone <repository-url>
cd AplikasiWebMethodSOAP

# Restore packages
dotnet restore

# Build project
dotnet build
```

---

## Menjalankan Aplikasi

```bash
# Jalankan aplikasi
dotnet run

# Atau dengan URL spesifik
dotnet run --urls "http://localhost:5225"
```

### Akses Aplikasi

| URL | Deskripsi |
|-----|-----------|
| `http://localhost:5225` | UI Banking (MVC) |
| `http://localhost:5225/BankingService.svc` | SOAP Service Endpoint |
| `http://localhost:5225/BankingService.svc?wsdl` | WSDL Documentation |

---

## SOAP Service API

### Namespace

- **Service Namespace:** `http://tempuri.org/`
- **Data Contract Namespace:** `http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models`

---

### 1. CreateAccount

Membuat akun banking baru.

**Operation:** `CreateAccount`

**Request:**
```xml
<CreateAccount xmlns="http://tempuri.org/">
  <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
    <q1:AccountHolderName>string</q1:AccountHolderName>
    <q1:AccountType>string</q1:AccountType>
    <q1:InitialBalance>decimal</q1:InitialBalance>
  </request>
</CreateAccount>
```

**Response:**
```xml
<CreateAccountResponse xmlns="http://tempuri.org/">
  <CreateAccountResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
    <a:AccountHolderName>string</a:AccountHolderName>
    <a:AccountNumber>string</a:AccountNumber>
    <a:AccountType>string</a:AccountType>
    <a:Balance>decimal</a:Balance>
    <a:CreatedDate>dateTime</a:CreatedDate>
  </CreateAccountResult>
</CreateAccountResponse>
```

**Validasi:**
- AccountHolderName: tidak boleh kosong
- InitialBalance: tidak boleh negatif
- AccountType: tidak boleh kosong

---

### 2. GetAccount

Mendapatkan detail akun berdasarkan nomor akun.

**Operation:** `GetAccount`

**Request:**
```xml
<GetAccount xmlns="http://tempuri.org/">
  <accountNumber>string</accountNumber>
</GetAccount>
```

**Response:**
```xml
<GetAccountResponse xmlns="http://tempuri.org/">
  <GetAccountResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
    <a:AccountHolderName>string</a:AccountHolderName>
    <a:AccountNumber>string</a:AccountNumber>
    <a:AccountType>string</a:AccountType>
    <a:Balance>decimal</a:Balance>
    <a:CreatedDate>dateTime</a:CreatedDate>
  </GetAccountResult>
</GetAccountResponse>
```

---

### 3. GetAllAccounts

Mendapatkan semua akun yang terdaftar.

**Operation:** `GetAllAccounts`

**Request:**
```xml
<GetAllAccounts xmlns="http://tempuri.org/" />
```

**Response:**
```xml
<GetAllAccountsResponse xmlns="http://tempuri.org/">
  <GetAllAccountsResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
    <a:Account>
      <a:AccountHolderName>string</a:AccountHolderName>
      <a:AccountNumber>string</a:AccountNumber>
      <a:AccountType>string</a:AccountType>
      <a:Balance>decimal</a:Balance>
      <a:CreatedDate>dateTime</a:CreatedDate>
    </a:Account>
  </GetAllAccountsResult>
</GetAllAccountsResponse>
```

---

### 4. Deposit

Menambahkan saldo ke akun.

**Operation:** `Deposit`

**Request:**
```xml
<Deposit xmlns="http://tempuri.org/">
  <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
    <q1:AccountNumber>string</q1:AccountNumber>
    <q1:Amount>decimal</q1:Amount>
  </request>
</Deposit>
```

**Response:**
```xml
<DepositResponse xmlns="http://tempuri.org/">
  <DepositResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
    <a:AccountHolderName>string</a:AccountHolderName>
    <a:AccountNumber>string</a:AccountNumber>
    <a:AccountType>string</a:AccountType>
    <a:Balance>decimal</a:Balance>
    <a:CreatedDate>dateTime</a:CreatedDate>
  </DepositResult>
</DepositResponse>
```

**Validasi:**
- Amount: harus lebih dari 0
- AccountNumber: harus ada

---

### 5. Withdraw

Mengurangi saldo dari akun.

**Operation:** `Withdraw`

**Request:**
```xml
<Withdraw xmlns="http://tempuri.org/">
  <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
    <q1:AccountNumber>string</q1:AccountNumber>
    <q1:Amount>decimal</q1:Amount>
  </request>
</Withdraw>
```

**Response:**
```xml
<WithdrawResponse xmlns="http://tempuri.org/">
  <WithdrawResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
    <a:AccountHolderName>string</a:AccountHolderName>
    <a:AccountNumber>string</a:AccountNumber>
    <a:AccountType>string</a:AccountType>
    <a:Balance>decimal</a:Balance>
    <a:CreatedDate>dateTime</a:CreatedDate>
  </WithdrawResult>
</WithdrawResponse>
```

**Validasi:**
- Amount: harus lebih dari 0
- AccountNumber: harus ada
- Saldo harus mencukupi

---

### 6. Transfer

Transfer saldo antar akun.

**Operation:** `Transfer`

**Request:**
```xml
<Transfer xmlns="http://tempuri.org/">
  <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
    <q1:Amount>decimal</q1:Amount>
    <q1:FromAccountNumber>string</q1:FromAccountNumber>
    <q1:ToAccountNumber>string</q1:ToAccountNumber>
  </request>
</Transfer>
```

**Response:**
```xml
<TransferResponse xmlns="http://tempuri.org/">
  <TransferResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
    <a:Success>boolean</a:Success>
    <a:Message>string</a:Message>
  </TransferResult>
</TransferResponse>
```

**Validasi:**
- Amount: harus lebih dari 0
- FromAccountNumber dan ToAccountNumber harus berbeda
- Kedua akun harus ada
- Saldo sumber harus mencukupi

---

### 7. GetTransactionHistory

Mendapatkan riwayat transaksi akun.

**Operation:** `GetTransactionHistory`

**Request:**
```xml
<GetTransactionHistory xmlns="http://tempuri.org/">
  <accountNumber>string</accountNumber>
</GetTransactionHistory>
```

**Response:**
```xml
<GetTransactionHistoryResponse xmlns="http://tempuri.org/">
  <GetTransactionHistoryResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
    <a:Transaction>
      <a:TransactionId>string</a:TransactionId>
      <a:FromAccountNumber>string</a:FromAccountNumber>
      <a:ToAccountNumber>string</a:ToAccountNumber>
      <a:Amount>decimal</a:Amount>
      <a:TransactionType>string</a:TransactionType>
      <a:TransactionDate>dateTime</a:TransactionDate>
      <a:Description>string</a:Description>
    </a:Transaction>
  </GetTransactionHistoryResult>
</GetTransactionHistoryResponse>
```

---

### 8. GetBalance

Mendapatkan saldo akun.

**Operation:** `GetBalance`

**Request:**
```xml
<GetBalance xmlns="http://tempuri.org/">
  <accountNumber>string</accountNumber>
</GetBalance>
```

**Response:**
```xml
<GetBalanceResponse xmlns="http://tempuri.org/">
  <GetBalanceResult>decimal</GetBalanceResult>
</GetBalanceResponse>
```

---

### 9. CloseAccount

Menutup akun (saldo harus 0).

**Operation:** `CloseAccount`

**Request:**
```xml
<CloseAccount xmlns="http://tempuri.org/">
  <accountNumber>string</accountNumber>
</CloseAccount>
```

**Response:**
```xml
<CloseAccountResponse xmlns="http://tempuri.org/">
  <CloseAccountResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
    <a:Success>boolean</a:Success>
    <a:Message>string</a:Message>
  </CloseAccountResult>
</CloseAccountResponse>
```

**Validasi:**
- AccountNumber: harus ada
- Saldo harus 0

---

## UI (User Interface)

### Halaman Utama
**URL:** `/` atau `/Banking`

Menampilkan menu fitur banking dalam bentuk card:
- Buat Akun
- Daftar Akun
- Setor
- Tarik
- Transfer
- Tutup Akun

### Daftar Halaman

| Halaman | URL | Deskripsi |
|---------|-----|-----------|
| Home | `/Banking` | Menu utama |
| Buat Akun | `/Banking/CreateAccount` | Form pembuatan akun |
| Detail Akun | `/Banking/AccountDetails?accountNumber=xxx` | Detail akun |
| Daftar Akun | `/Banking/AllAccounts` | Tabel semua akun |
| Setor | `/Banking/Deposit` | Form setor tunai |
| Tarik | `/Banking/Withdraw` | Form tarik tunai |
| Transfer | `/Banking/Transfer` | Form transfer |
| Riwayat | `/Banking/Transactions?accountNumber=xxx` | Riwayat transaksi |
| Tutup Akun | `/Banking/CloseAccount` | Form tutup akun |

---

## Contoh Request SOAP

### Membuat Akun Baru

```http
POST /BankingService.svc HTTP/1.1
Host: localhost:5225
Content-Type: text/xml
SOAPAction: "http://tempuri.org/IBankingService/CreateAccount"

<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <CreateAccount xmlns="http://tempuri.org/">
      <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <q1:AccountHolderName>John Doe</q1:AccountHolderName>
        <q1:AccountType>Savings</q1:AccountType>
        <q1:InitialBalance>1000000</q1:InitialBalance>
      </request>
    </CreateAccount>
  </soap:Body>
</soap:Envelope>
```

### Response Sukses

```xml
<?xml version="1.0" encoding="utf-8"?>
<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
  <s:Body>
    <CreateAccountResponse xmlns="http://tempuri.org/">
      <CreateAccountResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <a:AccountHolderName>John Doe</a:AccountHolderName>
        <a:AccountNumber>ACC-20260831-ABCD1234</a:AccountNumber>
        <a:AccountType>Savings</a:AccountType>
        <a:Balance>1000000</a:Balance>
        <a:CreatedDate>2026-08-31T16:00:00Z</a:CreatedDate>
      </CreateAccountResult>
    </CreateAccountResponse>
  </s:Body>
</s:Envelope>
```

### Transfer Antar Akun

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <Transfer xmlns="http://tempuri.org/">
      <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <q1:Amount>500000</q1:Amount>
        <q1:FromAccountNumber>ACC-20260831-ABCD1234</q1:FromAccountNumber>
        <q1:ToAccountNumber>ACC-20260831-EFGH5678</q1:ToAccountNumber>
      </request>
    </Transfer>
  </soap:Body>
</soap:Envelope>
```

---

## Format Response

### Account Object

| Field | Tipe | Deskripsi |
|-------|------|-----------|
| AccountNumber | string | Nomor akun (auto-generated) |
| AccountHolderName | string | Nama pemegang akun |
| AccountType | string | Tipe akun (Savings/Checking/Deposito) |
| Balance | decimal | Saldo akun |
| CreatedDate | dateTime | Tanggal pembuatan akun |

### Transaction Object

| Field | Tipe | Deskripsi |
|-------|------|-----------|
| TransactionId | string | ID transaksi (auto-generated) |
| FromAccountNumber | string | Nomor akun sumber |
| ToAccountNumber | string | Nomor akun tujuan |
| Amount | decimal | Jumlah transaksi |
| TransactionType | string | Tipe transaksi (DEPOSIT/WITHDRAWAL/TRANSFER) |
| TransactionDate | dateTime | Tanggal transaksi |
| Description | string | Keterangan transaksi |

### ServiceResponse Object

| Field | Tipe | Deskripsi |
|-------|------|-----------|
| Success | boolean | Status berhasil/gagal |
| Message | string | Pesan response |

---

## Error Handling

### Format Error Response

```xml
<?xml version="1.0" encoding="utf-8"?>
<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
  <s:Body>
    <s:Fault>
      <faultcode>s:Client</faultcode>
      <faultstring xml:lang="en-US">Pesan error</faultstring>
    </s:Fault>
  </s:Body>
</s:Envelope>
```

### Daftar Error

| Error | Penyebab |
|-------|----------|
| "Nama pemegang akun tidak boleh kosong" | AccountHolderName null/kosong |
| "Saldo awal tidak boleh negatif" | InitialBalance < 0 |
| "Tipe akun tidak boleh kosong" | AccountType null/kosong |
| "Jumlah deposit harus lebih dari 0" | Amount <= 0 |
| "Jumlah penarikan harus lebih dari 0" | Amount <= 0 |
| "Saldo tidak mencukupi" | Balance < Amount |
| "Akun dengan nomor xxx tidak ditemukan" | AccountNumber tidak ada |
| "Akun sumber dan tujuan tidak boleh sama" | FromAccountNumber == ToAccountNumber |
| "Akun tidak dapat ditutup karena masih memiliki saldo" | Balance > 0 saat tutup akun |

---

## Konfigurasi

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "BankingServiceUrl": "http://localhost:5225/BankingService.svc"
}
```

### Program.cs - Service Registration

```csharp
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, ServiceMetadataBehavior>();
builder.Services.AddSingleton<IBankingService, BankingService>();
```

### Program.cs - Endpoint Configuration

```csharp
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<BankingService>();
    serviceBuilder.AddServiceEndpoint<BankingService, IBankingService>(
        new BasicHttpBinding(), 
        "/BankingService.svc"
    );
});
```

---

## Data Models

### Account

```csharp
[DataContract]
public class Account
{
    [DataMember] public string AccountNumber { get; set; }
    [DataMember] public string AccountHolderName { get; set; }
    [DataMember] public decimal Balance { get; set; }
    [DataMember] public string AccountType { get; set; }
    [DataMember] public DateTime CreatedDate { get; set; }
}
```

### Transaction

```csharp
[DataContract]
public class Transaction
{
    [DataMember] public string TransactionId { get; set; }
    [DataMember] public string FromAccountNumber { get; set; }
    [DataMember] public string ToAccountNumber { get; set; }
    [DataMember] public decimal Amount { get; set; }
    [DataMember] public string TransactionType { get; set; }
    [DataMember] public DateTime TransactionDate { get; set; }
    [DataMember] public string Description { get; set; }
}
```

---

## Testing dengan Postman/cURL

### cURL Example

```bash
# Create Account
curl -X POST http://localhost:5225/BankingService.svc \
  -H "Content-Type: text/xml" \
  -H "SOAPAction: http://tempuri.org/IBankingService/CreateAccount" \
  -d '<?xml version="1.0" encoding="utf-8"?>
  <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
    <soap:Body>
      <CreateAccount xmlns="http://tempuri.org/">
        <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
          <q1:AccountHolderName>Test User</q1:AccountHolderName>
          <q1:AccountType>Savings</q1:AccountType>
          <q1:InitialBalance>500000</q1:InitialBalance>
        </request>
      </CreateAccount>
    </soap:Body>
  </soap:Envelope>'

# Get All Accounts
curl -X POST http://localhost:5225/BankingService.svc \
  -H "Content-Type: text/xml" \
  -H "SOAPAction: http://tempuri.org/IBankingService/GetAllAccounts" \
  -d '<?xml version="1.0" encoding="utf-8"?>
  <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
    <soap:Body>
      <GetAllAccounts xmlns="http://tempuri.org/" />
    </soap:Body>
  </soap:Envelope>'
```

---

## Catatan Penting

1. **Penyimpanan Data:** Data disimpan di memori (static lists). Data akan hilang ketika aplikasi di-restart.

2. **Thread Safety:** Aplikasi ini belum menerapkan thread-safe operations untuk penggunaan production.

3. **Single Instance:** BankingService di-register sebagai Singleton sehingga data tetap konsisten selama aplikasi berjalan.

4. **SOAP Namespace:** Pastikan namespace XML sesuai dengan spesifikasi WSDL untuk menghindah error deserialization.

---

## Lisensi

Project ini dibuat untuk keperluan edukasi dan pembelajaran.
