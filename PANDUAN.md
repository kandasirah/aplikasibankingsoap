# Panduan Memahami Sistem Banking SOAP

Panduan lengkap untuk memahami arsitektur, alur kerja, dan cara kerja sistem Banking SOAP Web Service.

## Daftar Isi

- [1. Konsep Dasar](#1-konsep-dasar)
  - [Apa itu SOAP?](#apa-itu-soap)
  - [Apa itu WebMethod?](#apa-itu-webmethod)
  - [Apa itu CoreWCF?](#apa-itu-corewcf)
- [2. Arsitektur Sistem](#2-arsitektur-sistem)
  - [Diagram Arsitektur](#diagram-arsitektur)
  - [Komponen Utama](#komponen-utama)
- [3. Alur Kerja Sistem](#3-alur-kerja-sistem)
  - [Flow Request-Response](#flow-request-response)
  - [Proses Pembuatan Akun](#proses-pembuatan-akun)
  - [Proses Transfer](#proses-transfer)
- [4. Penjelasan Kode](#4-penjelasan-kode)
  - [Service Contract (Interface)](#service-contract-interface)
  - [Service Implementation](#service-implementation)
  - [SOAP Client](#soap-client)
  - [MVC Controller](#mvc-controller)
  - [Razor View](#razor-view)
- [5. Cara Kerja SOAP Request](#5-cara-kerja-soap-request)
  - [SOAP Envelope](#soap-envelope)
  - [SOAP Header](#soap-header)
  - [SOAP Body](#soap-body)
  - [Namespace XML](#namespace-xml)
- [6. Data Flow](#6-data-flow)
  - [Models](#models)
  - [Static Data Storage](#static-data-storage)
  - [Serialization/Deserialization](#serializationdeserialization)
- [7. Testing Sistem](#7-testing-sistem)
  - [Testing via UI](#testing-via-ui)
  - [Testing via SOAP Request](#testing-via-soap-request)
  - [Testing via cURL](#testing-via-curl)
- [8. Troubleshooting](#8-troubleshooting)
  - [Error Umum](#error-umum)
  - [Debugging Tips](#debugging-tips)

---

## 1. Konsep Dasar

### Apa itu SOAP?

**SOAP (Simple Object Access Protocol)** adalah protokol komunikasi untuk bertukar pesan dalam format XML melalui HTTP.

```
┌──────────────┐     XML/SOAP      ┌──────────────┐
│   Client     │ ───────────────►  │   Server     │
│              │ ◄───────────────  │              │
└──────────────┘     XML/SOAP      └──────────────┘
```

**Karakteristik SOAP:**
- Menggunakan XML untuk format pesan
- Menggunakan HTTP/HTTPS sebagai transport
- Memiliki struktur terstandarisasi (Envelope, Header, Body)
- Platform independent (bisa digunakan di bahasa pemrograman apapun)

### Apa itu WebMethod?

**WebMethod** adalah atribut dalam ASP.NET yang menandakan sebuah method dapat diakses via web service (SOAP).

```csharp
[OperationContract]  // <-- Ini adalah WebMethod
public Task<Account> CreateAccountAsync(CreateAccountRequest request)
{
    // Implementation
}
```

### Apa itu CoreWCF?

**CoreWCF** adalah porting dari Windows Communication Foundation (WCF) ke .NET Core/.NET 5+. CoreWCF memungkinkan kita membuat SOAP Web Service di ASP.NET Core.

**Mengapa CoreWCF?**
- WCF original hanya tersedia di .NET Framework (Windows only)
- CoreWCF memungkinkan SOAP service di .NET Core (cross-platform)
- Mendukung BasicHttpBinding untuk kompatibilitas

---

## 2. Arsitektur Sistem

### Diagram Arsitektur

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              BROWSER (Client)                               │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                         Razor Views (HTML)                            │  │
│  │   - Index.cshtml, CreateAccount.cshtml, Deposit.cshtml, dll          │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                       │
                                       │ HTTP POST (Form Submission)
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         ASP.NET Core Web App                               │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                      BankingController                                │  │
│  │   - Menerima request dari View                                       │  │
│  │   - Memanggil BankingClient                                          │  │
│  │   - Mengembalikan View/Redirect                                      │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                       │                                     │
│                                       │ Method Call                         │
│                                       ▼                                     │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                        BankingClient                                  │  │
│  │   - Membuat SOAP Envelope (XML)                                      │  │
│  │   - Mengirim HTTP POST ke SOAP Service                               │  │
│  │   - Menerima response XML                                            │  │
│  │   - Parse XML ke Object .NET                                         │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                       │
                                       │ HTTP POST (SOAP XML)
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         SOAP Service (CoreWCF)                              │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                    IBankingService (Contract)                         │  │
│  │   - Definisi method yang tersedia                                    │  │
│  │   - [OperationContract] attribute                                    │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                       │                                     │
│                                       │ Implementation                      │
│                                       ▼                                     │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                   BankingService (Implementation)                     │  │
│  │   - Logika bisnis                                                    │  │
│  │   - Validasi input                                                   │  │
│  │   - Manipulasi data                                                  │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                       │                                     │
│                                       ▼                                     │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                     Static Data Storage                               │  │
│  │   - List<Account> _accounts                                          │  │
│  │   - List<Transaction> _transactions                                  │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Komponen Utama

| Komponen | File | Fungsi |
|----------|------|--------|
| Service Contract | `IBankingService.cs` | Interface yang mendefinisikan method SOAP |
| Service Implementation | `BankingService.cs` | Implementasi logika bisnis |
| SOAP Client | `BankingClient.cs` | Class untuk memanggil SOAP service |
| MVC Controller | `BankingController.cs` | Menangani request HTTP dari browser |
| Views | `Views/Banking/*.cshtml` | Template HTML untuk UI |
| Models | `BankingModels.cs` | Class untuk data structure |

---

## 3. Alur Kerja Sistem

### Flow Request-Response

```
┌─────────┐     ┌────────────┐     ┌─────────────┐     ┌──────────────┐
│ Browser │────►│ Controller │────►│ SOAP Client │────►│ SOAP Service │
│         │◄────│            │◄────│             │◄────│              │
└─────────┘     └────────────┘     └─────────────┘     └──────────────┘
     │               │                   │                    │
     │ 1. User       │ 2. Call Method    │ 3. SOAP Request    │ 4. Process
     │    Submit     │                   │                    │
     │               │                   │                    │
     │ 7. Display    │ 6. Return View    │ 5. SOAP Response   │ 5. Return
     │    Result     │                   │                    │    Result
```

### Proses Pembuatan Akun

```
User mengisi form (Nama, Tipe, Saldo Awal)
              │
              ▼
┌─────────────────────────────┐
│  Browser POST ke Controller │
│  /Banking/CreateAccount     │
└─────────────────────────────┘
              │
              ▼
┌─────────────────────────────┐
│  BankingController          │
│  - Menerima CreateAccountRequest
│  - Memanggil BankingClient  │
└─────────────────────────────┘
              │
              ▼
┌─────────────────────────────┐
│  BankingClient              │
│  - Membuat SOAP Envelope    │
│  - Menambahkan namespace    │
│  - HTTP POST ke service     │
└─────────────────────────────┘
              │
              ▼
┌─────────────────────────────┐
│  BankingService             │
│  - Validasi input           │
│  - Generate AccountNumber   │
│  - Simpan ke _accounts      │
│  - Return Account object    │
└─────────────────────────────┘
              │
              ▼
┌─────────────────────────────┐
│  BankingClient              │
│  - Parse XML response       │
│  - Convert ke Account object│
│  - Return ke Controller     │
└─────────────────────────────┘
              │
              ▼
┌─────────────────────────────┐
│  BankingController          │
│  - Set TempData success     │
│  - Redirect ke AccountDetails│
└─────────────────────────────┘
              │
              ▼
┌─────────────────────────────┐
│  Browser menampilkan        │
│  detail akun yang dibuat    │
└─────────────────────────────┘
```

### Proses Transfer

```
User mengisi form (Dari Akun, Ke Akun, Jumlah)
              │
              ▼
┌─────────────────────────────┐
│  BankingController          │
│  .Transfer(TransferRequest) │
└─────────────────────────────┘
              │
              ▼
┌─────────────────────────────┐
│  BankingClient              │
│  - Membuat SOAP Envelope:   │
│    <Transfer>               │
│      <Amount>500000</Amount>│
│      <FromAccountNumber>... │
│      <ToAccountNumber>...   │
│    </Transfer>              │
└─────────────────────────────┘
              │
              ▼
┌─────────────────────────────┐
│  BankingService             │
│  - Validasi:                │
│    • Akun sumber ada?       │
│    • Akun tujuan ada?       │
│    • Saldo mencukupi?       │
│    • Bukan akun yang sama?  │
│  - Kurangi saldo sumber     │
│  - Tambah saldo tujuan      │
│  - Catat transaksi          │
│  - Return ServiceResponse   │
└─────────────────────────────┘
              │
              ▼
┌─────────────────────────────┐
│  Controller menampilkan     │
│  pesan sukses/error         │
└─────────────────────────────┘
```

---

## 4. Penjelasan Kode

### Service Contract (Interface)

**File:** `Contracts/IBankingService.cs`

```csharp
[ServiceContract]  // Menandakan ini adalah SOAP Service Contract
public interface IBankingService
{
    [OperationContract]  // Method ini bisa diakses via SOAP
    Task<Account> CreateAccountAsync(CreateAccountRequest request);
    
    [OperationContract]
    Task<Account?> GetAccountAsync(string accountNumber);
    
    // ... method lainnya
}
```

**Penjelasan:**
- `[ServiceContract]` - Attribute yang menandakan interface ini adalah SOAP service
- `[OperationContract]` - Attribute yang menandakan method bisa diakses via SOAP
- Setiap method akan menjadi "WebMethod" yang bisa dipanggil dari client

### Service Implementation

**File:** `Services/BankingService.cs`

```csharp
public class BankingService : IBankingService
{
    // Static list untuk menyimpan data di memori
    private static readonly List<Account> _accounts = new();
    private static readonly List<Transaction> _transactions = new();

    public Task<Account> CreateAccountAsync(CreateAccountRequest request)
    {
        // 1. Validasi input
        if (string.IsNullOrWhiteSpace(request.AccountHolderName))
            throw new FaultException("Nama pemegang akun tidak boleh kosong");

        // 2. Buat object Account baru
        var account = new Account
        {
            AccountNumber = GenerateAccountNumber(),  // Auto-generate
            AccountHolderName = request.AccountHolderName,
            Balance = request.InitialBalance,
            AccountType = request.AccountType,
            CreatedDate = DateTime.UtcNow
        };

        // 3. Simpan ke static list
        _accounts.Add(account);

        // 4. Return hasil
        return Task.FromResult(account);
    }
}
```

**Penjelasan:**
- Data disimpan di `static List<>` sehingga tetap ada selama aplikasi berjalan
- `FaultException` digunakan untuk mengirim error ke client SOAP
- Setiap method async mengembalikan `Task<T>`

### SOAP Client

**File:** `Controllers/BankingClient.cs`

```csharp
public class BankingClient : IDisposable
{
    private readonly string _serviceUrl;
    private readonly HttpClient _httpClient;

    public async Task<Account> CreateAccountAsync(CreateAccountRequest request)
    {
        // 1. Buat SOAP Envelope (XML)
        var requestContent = $@"<request xmlns:q1=""{DataNamespace}"">
            <q1:AccountHolderName>{request.AccountHolderName}</q1:AccountHolderName>
            <q1:AccountType>{request.AccountType}</q1:AccountType>
            <q1:InitialBalance>{request.InitialBalance}</q1:InitialBalance>
        </request>";

        // 2. Parse response XML ke object
        var doc = await CallServiceAsync("CreateAccount", requestContent);
        var result = doc.Descendants(ns + "CreateAccountResult").FirstOrDefault();
        
        // 3. Convert XElement ke Account object
        return new Account
        {
            AccountHolderName = result.Element(dataNs + "AccountHolderName")?.Value,
            AccountNumber = result.Element(dataNs + "AccountNumber")?.Value,
            // ... property lainnya
        };
    }
}
```

**Penjelasan:**
- SOAP Client membuat XML SOAP Envelope secara manual
- Menggunakan `HttpClient` untuk mengirim HTTP POST
- Parse response XML menggunakan `XDocument` dan `XNamespace`
- Convert XElement ke object .NET secara manual

### MVC Controller

**File:** `Controllers/BankingController.cs`

```csharp
public class BankingController : Controller
{
    [HttpPost]
    public async Task<IActionResult> CreateAccount(CreateAccountRequest request)
    {
        try
        {
            // 1. Panggil SOAP Client
            using var client = new BankingClient(ServiceUrl);
            var account = await client.CreateAccountAsync(request);
            
            // 2. Set pesan sukses
            TempData["Success"] = $"Akun berhasil dibuat: {account.AccountNumber}";
            
            // 3. Redirect ke halaman detail
            return RedirectToAction(nameof(AccountDetails), 
                new { accountNumber = account.AccountNumber });
        }
        catch (Exception ex)
        {
            // 4. Handle error
            TempData["Error"] = ex.Message;
            return View(request);
        }
    }
}
```

**Penjelasan:**
- Controller menerima request dari form HTML
- Memanggil `BankingClient` untuk berkomunikasi dengan SOAP Service
- Menggunakan `TempData` untuk mengirim pesan antar redirect
- Mengembalikan View atau Redirect

### Razor View

**File:** `Views/Banking/CreateAccount.cshtml`

```html
@model AplikasiWebMethodSOAP.Models.CreateAccountRequest

<form asp-action="CreateAccount" method="post">
    <input asp-for="AccountHolderName" class="form-control" />
    <select asp-for="AccountType" class="form-select">
        <option value="Savings">Tabungan</option>
    </select>
    <input asp-for="InitialBalance" type="number" />
    <button type="submit">Buat Akun</button>
</form>
```

**Penjelasan:**
- `@model` menentukan tipe data model yang digunakan
- `asp-for` menghubungkan input dengan property model
- `asp-action` menentukan action method yang dipanggil saat submit
- Tag helpers menghasilkan HTML dengan atribut `name` yang sesuai

---

## 5. Cara Kerja SOAP Request

### SOAP Envelope

SOAP Envelope adalah "pembungkus" pesan SOAP. Struktur:

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Header>
    <!-- Optional: Authentication, Transaction ID, dll -->
  </soap:Header>
  <soap:Body>
    <!-- Isi pesan (Request atau Response) -->
  </soap:Body>
</soap:Envelope>
```

### SOAP Header

Header bersifat optional, biasanya berisi:
- Authentication token
- Transaction ID
- Routing information

```xml
<soap:Header>
  <AuthHeader>
    <Username>admin</Username>
    <Password>secret</Password>
  </AuthHeader>
</soap:Header>
```

### SOAP Body

Body berisi pesan utama (request atau response):

**Request Body:**
```xml
<soap:Body>
  <CreateAccount xmlns="http://tempuri.org/">
    <request>
      <AccountHolderName>John Doe</AccountHolderName>
    </request>
  </CreateAccount>
</soap:Body>
```

**Response Body:**
```xml
<soap:Body>
  <CreateAccountResponse xmlns="http://tempuri.org/">
    <CreateAccountResult>
      <AccountNumber>ACC-12345</AccountNumber>
      <Balance>1000000</Balance>
    </CreateAccountResult>
  </CreateAccountResponse>
</soap:Body>
```

### Namespace XML

Namespace sangat penting dalam SOAP untuk menghindari konflik nama:

| Namespace | Prefix | Fungsi |
|-----------|--------|--------|
| `http://schemas.xmlsoap.org/soap/envelope/` | `soap:` | SOAP Envelope standard |
| `http://tempuri.org/` | (default) | Service namespace |
| `http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models` | `q1:`, `a:` | Data contract namespace |

**Contoh penggunaan namespace:**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <!-- Default namespace untuk service -->
    <CreateAccount xmlns="http://tempuri.org/">
      <!-- Data contract namespace untuk model -->
      <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <q1:AccountHolderName>John Doe</q1:AccountHolderName>
      </request>
    </CreateAccount>
  </soap:Body>
</soap:Envelope>
```

---

## 6. Data Flow

### Models

**File:** `Models/BankingModels.cs`

```csharp
[DataContract]  // Menandakan class bisa di-serialize ke XML
public class Account
{
    [DataMember]  // Property ini akan di-include dalam XML
    public string AccountNumber { get; set; }
    
    [DataMember]
    public string AccountHolderName { get; set; }
    
    [DataMember]
    public decimal Balance { get; set; }
    
    [DataMember]
    public string AccountType { get; set; }
    
    [DataMember]
    public DateTime CreatedDate { get; set; }
}
```

**Proses Serialization:**
```
Account object ──► XML
{
    "AccountNumber": "ACC-12345",      ──►  <AccountNumber>ACC-12345</AccountNumber>
    "AccountHolderName": "John Doe",   ──►  <AccountHolderName>John Doe</AccountHolderName>
    "Balance": 1000000                 ──►  <Balance>1000000</Balance>
}
```

### Static Data Storage

```csharp
public class BankingService : IBankingService
{
    // Static = shared across all instances & requests
    private static readonly List<Account> _accounts = new();
    private static readonly List<Transaction> _transactions = new();
}
```

**Penjelasan:**
- `static` berarti data ada di level class, bukan level instance
- Data tetap ada selama aplikasi berjalan
- Data hilang ketika aplikasi di-restart
- **Tidak cocok untuk production** (gunakan database)

### Serialization/Deserialization

**Serialization (Object → XML):**
```
Account object
    │
    ▼
XmlSerializer
    │
    ▼
<Account>
    <AccountNumber>ACC-123</AccountNumber>
    <AccountHolderName>John</AccountHolderName>
</Account>
```

**Deserialization (XML → Object):**
```xml
<Account>
    <AccountNumber>ACC-123</AccountNumber>
    <AccountHolderName>John</AccountHolderName>
</Account>
    │
    ▼
XDocument.Parse()
    │
    ▼
Account object
```

---

## 7. Testing Sistem

### Testing via UI

1. **Buka browser:** `http://localhost:5225`
2. **Klik "Buat Akun"**
3. **Isi form:**
   - Nama: "John Doe"
   - Tipe: "Savings"
   - Saldo Awal: 1000000
4. **Klik "Buat Akun"**
5. **Hasil:** Redirect ke halaman detail akun

### Testing via SOAP Request

**Menggunakan Postman:**

1. **Method:** POST
2. **URL:** `http://localhost:5225/BankingService.svc`
3. **Headers:**
   - `Content-Type: text/xml`
   - `SOAPAction: "http://tempuri.org/IBankingService/CreateAccount"`
4. **Body (raw XML):**
```xml
<?xml version="1.0" encoding="utf-8"?>
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
</soap:Envelope>
```

### Testing via cURL

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

## 8. Troubleshooting

### Error Umum

| Error | Penyebab | Solusi |
|-------|----------|--------|
| "Nama pemegang akun tidak boleh kosong" | Field nama tidak terkirim | Pastikan `name="AccountHolderName"` di HTML |
| "Could not find service" | Service tidak ter-register | Cek `AddService<BankingService>()` di Program.cs |
| XML namespace error | Namespace tidak sesuai | Pastikan namespace sesuai WSDL |
| 404 Not Found | URL salah | Pastikan endpoint `/BankingService.svc` |
| Deserialization error | Format XML salah | Cek struktur XML response |

### Debugging Tips

1. **Lihat WSDL:**
   ```
   http://localhost:5225/BankingService.svc?wsdl
   ```

2. **Lihat XSD Schema:**
   ```
   http://localhost:5225/BankingService.svc?xsd=xsd0
   http://localhost:5225/BankingService.svc?xsd=xsd2
   ```

3. **Logging SOAP Request/Response:**
   ```csharp
   // Di BankingClient.cs, tambahkan logging:
   Console.WriteLine($"SOAP Request: {soapEnvelope}");
   Console.WriteLine($"SOAP Response: {responseContent}");
   ```

4. **Gunakan Postman/Insomnia:**
   - Test SOAP request langsung tanpa UI
   - Lihat raw XML response

5. **Cek Network Tab (Browser):**
   - Buka Developer Tools (F12)
   - Lihat Network tab untuk melihat request/response

---

## Ringkasan

### Alur Singkat:

```
User → Browser → Controller → BankingClient → SOAP Service → Data Storage
         ▲          │              │               │
         │          │              │               │
         └──────────┴──────────────┴───────────────┘
                    Response (XML → Object → HTML)
```

### Poin Penting:

1. **SOAP** menggunakan XML untuk pertukaran data
2. **CoreWCF** menyediakan SOAP service di .NET Core
3. **BankingClient** mengirim HTTP POST dengan SOAP Envelope
4. **Controller** menangani request dari browser
5. **View** menampilkan UI ke user
6. **Data** disimpan di static list (memory)

---

## Referensi

- [CoreWCF Documentation](https://github.com/CoreWCF/CoreWCF)
- [SOAP Specification](https://www.w3.org/TR/soap/)
- [ASP.NET Core MVC](https://docs.microsoft.com/aspnet/core/mvc)
- [WCF Services](https://docs.microsoft.com/dotnet/framework/wcf/)
