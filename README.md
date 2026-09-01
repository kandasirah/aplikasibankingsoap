# AplikasiWebMethodSOAP

Aplikasi web perbankan berbasis SOAP (Simple Object Access Protocol) yang dibangun dengan
**.NET 8.0** dan **CoreWCF**. Proyek ini merupakan contohimplementasi lengkap layanan SOAP
banking — dari kontrak (`ServiceContract`), implementasi, klien SOAP hingga tiga lapis
presentasi: **SOAP endpoint**, **REST API**, dan **MVC web UI**.

---

## Daftar Isi

1. [Spesifikasi & Teknologi](#spesifikasi--teknologi)
2. [Arsitektur](#arsitektur)
3. [Struktur Proyek](#struktur-proyek)
4. [Fitur](#fitur)
5. [Referensi SOAP Service API](#referensi-soap-service-api)
6. [Referensi REST API](#referensi-rest-api)
7. [Routing MVC UI](#routing-mvc-ui)
8. [Model / Data Contract](#model--data-contract)
9. [Cara Menjalankan](#cara-menjalankan)
10. [Konfigurasi](#konfigurasi)
11. [CI/CD](#cicd)
12. [Batasan & Catatan](#batasan--catatan)

---

## Spesifikasi & Teknologi

| Komponen            | Teknologi                                         |
|---------------------|---------------------------------------------------|
| Runtime             | .NET 8.0                                          |
| Framework Web       | ASP.NET Core 8.0                                  |
| SOAP Stack          | CoreWCF 1.9.1 (`CoreWCF.Http`, `CoreWCF.Primitives`) |
| SOAP Protokol       | HTTP + SOAP 1.1 (BasicHttpBinding)                |
| REST API            | ASP.NET Core Controllers + Swashbuckle Swagger 6.5.0 |
| MVC UI              | ASP.NET Core MVC + Razor Views                    |
| Styling             | Bootstrap 5.3, Font Awesome 6.4                   |
| Dependency Injection| ASP.NET Core built-in DI                          |
| Build               | MSBuild / `dotnet` CLI                            |
| CI                  | GitHub Actions (`.github/workflows/dotnet.yml`)   |

### Package-utama (`*.csproj`)

```xml
<PackageReference Include="CoreWCF.Http" Version="1.9.1" />
<PackageReference Include="CoreWCF.Primitives" Version="1.9.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="System.ServiceModel.Primitives" Version="8.1.1" />
```

---

## Arsitektur

Proyek ini mengikuti pola **3-tier / layered**, dengan satu proyek .NET yang memisahkan
tanggung jawab melalui folder dan interface:

```
                            ┌──────────────────────────────────────────────────┐
                            │  LAYER 1 — PRESENTATION  (Konsumen)              │
                            │  ┌──────────┐  ┌───────────┐  ┌─────────────┐  │
                            │  │ MVC UI   │  │  REST API │  │ Swagger UI  │  │
                            │  │ Browser  │  │ /api      │  │ /swagger    │  │
                            │  └────┬─────┘  └─────┬─────┘  └─────────────┘  │
                            └──────┼────────────────┼────────────────────────┘
                                   │   HTTP         │ HTTP
                                   ▼                ▼
                          ┌─────────────────────────────────────┐
                          │  BankingClient  (HTTP + manual XML) │
                          │  Controllers/BankingClient.cs        │
                          └─────────────────────────────────────┘
                                   │   SOAP over HTTP (POST /BankingService.svc)
                                   ▼
                            ┌────────────────────────────────────┐
                            │ LAYER 2 — SOAP SERVICE  (Penyedia)   │
                            │  ┌──────────────────────┐           │
                            │  │ BankingService        │           │
                            │  │ Services/Banking*.cs  │           │
                            │  └─────┬───────────┬────┘           │
                            │        │ implements│ defines        │
                            │        ▼           ▼               │
                            │  IBankingService   BankingModels    │
                            │  Contracts         (DataContracts)   │
                            └──────┬───────────────────────────────┘
                                   │ operates on
                                   ▼
                            ┌────────────────────────────────────┐
                            │  LAYER 3 — DOMAIN / STATE           │
                            │  In-Memory Store (static lists)     │
                            │  • _accounts  : List<Account>      │
                            │  • _transactions: List<Transaction>│
                            └────────────────────────────────────┘
```

**Alur Permintaan Sistem**

```
  Pengguna / HTTP Client
  ──────────────────────────────────────────────────────────────────

  Web UI (MVC)
  ───────────
  Browser ──GET /Banking/──────────► ASP.NET Core Host
                                  └─ BankingController (MVC)
                                        │
                                        ├── new BankingClient(url)
                                        │       │ build SOAP envelope (XML)
                                        │       ▼
                                        │   POST /BankingService.svc
                                        │       │
                                        │       ▼
                                        └── BankingService (CoreWCF)
                                                  │ IBankingService.xxx
                                                  │  validate + mutate in-mem
                                                  ▼
                                            SOAP response (XML)
                                                  │
                                          BankingClient parses XML
                                                  │
                                          Controller → View / redirect
                                                  │
                                                HTML ──► Browser

  REST API
  ───────
  Client ──POST /api/BankingApi/deposit──► BankingApiController
                                        │
                                        ├── new BankingClient(url)   (id. SOAP)
                                        │       └──► POST /BankingService.svc
                                        │               └──► BankingService
                                        │                      └──► response XML
                                        │
                                        └── JSON 200 / 400  ──► Client
```

**Skema Relasi Model Data (Data Contract)**

```
  ┌──────────────┐          1      ┌──────────────┐
  │   Account    │◄─────────────── │ Transaction  │
  └──────────────┘    melibatkan  └──────────────┘
  AccountNumber                   TransactionId
  AccountHolderName               FromAccountNumber  (debit, 0..1 Account)
  Balance                         ToAccountNumber    (kredit, 0..1 Account)
  AccountType                     Amount
  CreatedDate                     TransactionType
                                  TransactionDate
                                  Description

  Request DTOs  ──►  operasi SOAP yang sesuaian:

  CreateAccountRequest ──► CreateAccountAsync()
  DepositRequest         ──► DepositAsync()
  WithdrawRequest        ──► WithdrawAsync()
  TransferRequest        ──► TransferAsync()
  (string accountNumber) ──► GetAccountAsync()
                          ──► GetTransactionHistoryAsync()
                          ──► GetBalanceAsync()
                          ──► CloseAccountAsync()

  ServiceResponse ──► TransferAsync() & CloseAccountAsync()
  (Success, Message)
```

**Pola kunci:**

- **SOAP-first**: `IBankingService` (di `Contracts/`) mendefinisikan kontrak operasi
  (`[ServiceContract]` / `[OperationContract]`), dan `BankingService` di `Services/`
  menyediakan implementasinya.
- **Dual-presentsi konsumen**: `BankingController` (MVC) dan `BankingApiController`
  (REST) **keduanya** mengonsumsi SOAP *secara internal* melalui `BankingClient` —
  sebuah klien SOAP ringan yang tidak memakai proxy yang diseramkan (`svcutil`),
  melainkan membangun *envelope* SOAP manual dan mem-parsing XML balasan dengan
  `System.Xml.Linq`.
- **Injeksi ketergantungan**: `IBankingService` didaftarkan sebagai singleton
  (`Program.cs:14`) dan di-resolve otomatis CoreWCF pada saat hosting endpoint SOAP.

---

## Struktur Proyek

```
AplikasiWebMethodSOAP/
├── Program.cs                    # Entry point: DI, routing, registrasi SOAP
├── AplikasiWebMethodSOAP.csproj  # Definisi proyek (.NET 8.0, package refs)
├── appsettings.json              # Konfigurasi (BankingServiceUrl, logging)
├── appsettings.Development.json  # Override konfigurasi development
├── AplikasiWebMethodSOAP.http    # Berkas permintaan HTTP (VS) - sisa template
├── Properties/
│   └── launchSettings.json       # Profil jalankan (http/https/IIS Express)
├── Contracts/
│   └── IBankingService.cs        # Kontrak SOAP (ServiceContract)
├── Services/
│   └── BankingService.cs         # Implementasi layanan (in-memory store)
├── Controllers/
│   ├── BankingController.cs      # MVC controller (web UI)
│   ├── BankingApiController.cs   # REST API controller
│   └── BankingClient.cs          # Klien SOAP manual (HttpClient + XML)
├── Models/
│   └── BankingModels.cs          # DataContract: Account, Transaction, dsb.
├── Views/
│   ├── _ViewStart.cshtml         # Layout default
│   ├── _ViewImports.cshtml       # Tag helpers & using
│   ├── Shared/
│   │   └── _Layout.cshtml        # Layout master (Bootstrap)
│   └── Banking/
│       ├── Index.cshtml          # Beranda (menu fitur)
│       ├── CreateAccount.cshtml
│       ├── AccountDetails.cshtml
│       ├── AllAccounts.cshtml
│       ├── Deposit.cshtml
│       ├── Withdraw.cshtml
│       ├── Transfer.cshtml
│       ├── Transactions.cshtml
│       └── CloseAccount.cshtml
├── .github/
│   └── workflows/
│       └── dotnet.yml            # CI/CD GitHub Actions
└── README.md                     # <-- berkas ini
```

---

## Fitur

Aplikasi ini menyediakan sistem perbankan sederhana dengan operasiCRUD lengkap:

| No | Fitur              | Digambar (MVC) | Digambar (REST) | Digambar (SOAP) |
|----|--------------------|:--------------:|:---------------:|:---------------:|
| 1  | **Buat Akun**      | ✅             | ✅              | ✅              |
| 2  | **Lihat Detail Akun** | ✅          | ✅              | ✅              |
| 3  | **Daftar Semua Akun** | ✅          | ✅              | ✅              |
| 4  | **Setor (Deposit)** | ✅            | ✅              | ✅              |
| 5  | **Tarik (Withdraw)** | ✅            | ✅              | ✅              |
| 6  | **Transfer**       | ✅             | ✅              | ✅              |
| 7  | **Riwayat Transaksi** | ✅           | ✅              | ✅              |
| 8  | **Cek Saldo**      | ✅             | ✅              | ✅              |
| 9  | **Tutup Akun**     | ✅             | ✅              | ✅              |

### Logika Bisnis Utama

- **Nomor akun** dibangkitkan otomatis: `ACC-{yyyyMMdd}-{uniq-8-char}`
- **Nomor transaksi** dibangkitkan otomatis: `TRX-{yyyyMMddHHmmss}-{counter:000000}`
- **Setoran awal** > 0 mencatat transaksi `DEPOSIT` dari `SYSTEM`
- Transfer memindahkan dana antar-akun secara atom (gunakan dalam transaksi yang
  sama logisnya karena penyimpanan in-memory)
- Penutupan akun hanya diizinkan saat **saldo = 0**
- Semua validasi error melempar `FaultException` dengan pesan Indonesia

---

## Referensi SOAP Service API

- **Endpoint**: `http://localhost:5225/BankingService.svc`
- **Binding**: `BasicHttpBinding` (SOAP 1.1)
- **Namespace operasi**: `http://tempuri.org/`
- **Namespace data kontrak**: `http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models`

| #  | Operation                   | Input                                    | Output                | Deskripsi                                   |
|----|-----------------------------|------------------------------------------|-----------------------|---------------------------------------------|
| 1  | `CreateAccountAsync`        | `[BodyParam] CreateAccountRequest`       | `Account`             | Membuat akun baru                           |
| 2  | `GetAccountAsync`           | `[BodyParam] string accountNumber`       | `Account?`            | Mengambil detail akun (nullable)            |
| 3  | `GetAllAccountsAsync`       | —                                        | `List<Account>`       | Mengembalikan semua akun                    |
| 4  | `DepositAsync`              | `[BodyParam] DepositRequest`             | `Account`             | Melakukan setoran                           |
| 5  | `WithdrawAsync`             | `[BodyParam] WithdrawRequest`            | `Account`             | Melakukan penarikan                         |
| 6  | `TransferAsync`             | `[BodyParam] TransferRequest`            | `ServiceResponse`     | Transfer antar akun                         |
| 7  | `GetTransactionHistoryAsync`| `[BodyParam] string accountNumber`       | `List<Transaction>`   | Riwayat transaksi akun                      |
| 8  | `GetBalanceAsync`           | `[BodyParam] string accountNumber`       | `decimal`             | Saldo akun                                  |
| 9  | `CloseAccountAsync`         | `[BodyParam] string accountNumber`       | `ServiceResponse`     | Menutup akun (syarat: saldo 0)              |

### Contoh Permintaan & Respons SOAP — Operasi Lengkap

Di bawah ini contoh **request (SOAP Envelope)** dan **response** untuk tiap operasi.
Untuk semua request, header HTTP standar adalah:

```
Content-Type: text/xml; charset=utf-8
SOAPAction:   "http://tempuri.org/IBankingService/<OperationName>"
```

Namespace data `Account`, `Transaction`, dan semua `_Request` berada di
`http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models`.

---

#### 1. `CreateAccount` — Membuat Akun Baru

**Request:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <CreateAccount xmlns="http://tempuri.org/">
      <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <q1:AccountHolderName>Budi Santoso</q1:AccountHolderName>
        <q1:AccountType>Giro</q1:AccountType>
        <q1:InitialBalance>1000000</q1:InitialBalance>
      </request>
    </CreateAccount>
  </soap:Body>
</soap:Envelope>
```

**Response** (berisi `CreateAccountResult`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <CreateAccountResponse xmlns="http://tempuri.org/">
      <CreateAccountResult xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models"
                           xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        <q1:AccountHolderName>Budi Santoso</q1:AccountHolderName>
        <q1:AccountNumber>ACC-20240101-7F3A9B2C</q1:AccountNumber>
        <q1:AccountType>Giro</q1:AccountType>
        <q1:Balance>1000000</q1:Balance>
        <q1:CreatedDate>2024-01-01T04:27:33.1094026Z</q1:CreatedDate>
      </CreateAccountResult>
    </CreateAccountResponse>
  </soap:Body>
</soap:Envelope>
```

---

#### 2. `GetAccount` — Mendapatkan Detail Akun

**Request:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetAccount xmlns="http://tempuri.org/">
      <accountNumber>ACC-20240101-7F3A9B2C</accountNumber>
    </GetAccount>
  </soap:Body>
</soap:Envelope>
```

**Response** (akun ditemukan):

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetAccountResponse xmlns="http://tempuri.org/">
      <GetAccountResult xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models"
                        xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        <q1:AccountHolderName>Budi Santoso</q1:AccountHolderName>
        <q1:AccountNumber>ACC-20240101-7F3A9B2C</q1:AccountNumber>
        <q1:AccountType>Giro</q1:AccountType>
        <q1:Balance>1000000</q1:Balance>
        <q1:CreatedDate>2024-01-01T04:27:33.1094026Z</q1:CreatedDate>
      </GetAccountResult>
    </GetAccountResponse>
  </soap:Body>
</soap:Envelope>
```

> Jika akun tidak ditemukan, elemen `GetAccountResult` berisi nilai `null`/kosong
> (`xsi:nil="true"`).

---

#### 3. `GetAllAccounts` — Mendaftar Semua Akun

**Request (tidak memakai body payload):**

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetAllAccounts xmlns="http://tempuri.org/" />
  </soap:Body>
</soap:Envelope>
```

**Response** (berisi senarai elemen `Account` di dalam `GetAllAccountsResult`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetAllAccountsResponse xmlns="http://tempuri.org/">
      <GetAllAccountsResult xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models"
                            xmlns:i="http://www.w3.org/2001/XMLSchema-instance"
                            xmlns:a="http://schemas.microsoft.com/2006/08/addressing">
        <q1:Account>
          <q1:AccountHolderName>Budi Santoso</q1:AccountHolderName>
          <q1:AccountNumber>ACC-20240101-7F3A9B2C</q1:AccountNumber>
          <q1:AccountType>Giro</q1:AccountType>
          <q1:Balance>500000</q1:Balance>
          <q1:CreatedDate>2024-01-01T04:27:33.1094026Z</q1:CreatedDate>
        </q1:Account>
        <q1:Account>
          <q1:AccountHolderName>Siti Aminah</q1:AccountHolderName>
          <q1:AccountNumber>ACC-20240102-1C5D4E7F</q1:AccountNumber>
          <q1:AccountType>Tabungan</q1:AccountType>
          <q1:Balance>2000000</q1:Balance>
          <q1:CreatedDate>2024-01-02T01:10:05.0000000Z</q1:CreatedDate>
        </q1:Account>
      </GetAllAccountsResult>
    </GetAllAccountsResponse>
  </soap:Body>
</soap:Envelope>
```

---

#### 4. `Deposit` — Setor Tunai ke Akun

**Request:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <Deposit xmlns="http://tempuri.org/">
      <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <q1:AccountNumber>ACC-20240101-7F3A9B2C</q1:AccountNumber>
        <q1:Amount>250000</q1:Amount>
      </request>
    </Deposit>
  </soap:Body>
</soap:Envelope>
```

**Response** (akun setelah deposit):

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <DepositResponse xmlns="http://tempuri.org/">
      <DepositResult xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models"
                     xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        <q1:AccountHolderName>Budi Santoso</q1:AccountHolderName>
        <q1:AccountNumber>ACC-20240101-7F3A9B2C</q1:AccountNumber>
        <q1:AccountType>Giro</q1:AccountType>
        <q1:Balance>1250000</q1:Balance>
        <q1:CreatedDate>2024-01-01T04:27:33.1094026Z</q1:CreatedDate>
      </DepositResult>
    </DepositResponse>
  </soap:Body>
</soap:Envelope>
```

---

#### 5. `Withdraw` — Tarik Tunai dari Akun

**Request:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <Withdraw xmlns="http://tempuri.org/">
      <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <q1:AccountNumber>ACC-20240101-7F3A9B2C</q1:AccountNumber>
        <q1:Amount>300000</q1:Amount>
      </request>
    </Withdraw>
  </soap:Body>
</soap:Envelope>
```

**Response** (akun setelah penarikan):

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <WithdrawResponse xmlns="http://tempuri.org/">
      <WithdrawResult xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models"
                      xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        <q1:AccountHolderName>Budi Santoso</q1:AccountHolderName>
        <q1:AccountNumber>ACC-20240101-7F3A9B2C</q1:AccountNumber>
        <q1:AccountType>Giro</q1:AccountType>
        <q1:Balance>950000</q1:Balance>
        <q1:CreatedDate>2024-01-01T04:27:33.1094026Z</q1:CreatedDate>
      </WithdrawResult>
    </WithdrawResponse>
  </soap:Body>
</soap:Envelope>
```

---

#### 6. `Transfer` — Transfer Antar Akun

**Request:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <Transfer xmlns="http://tempuri.org/">
      <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <q1:Amount>400000</q1:Amount>
        <q1:FromAccountNumber>ACC-20240101-7F3A9B2C</q1:FromAccountNumber>
        <q1:ToAccountNumber>ACC-20240102-1C5D4E7F</q1:ToAccountNumber>
      </request>
    </Transfer>
  </soap:Body>
</soap:Envelope>
```

**Response** (berisi `ServiceResponse`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <TransferResponse xmlns="http://tempuri.org/">
      <TransferResult xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models"
                      xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        <q1:Message>Transfer sebesar Rp400.000,00 berhasil dari ACC-20240101-7F3A9B2C ke ACC-20240102-1C5D4E7F</q1:Message>
        <q1:Success>true</q1:Success>
      </TransferResult>
    </TransferResponse>
  </soap:Body>
</soap:Envelope>
```

---

#### 7. `GetTransactionHistory` — Riwayat Transaksi Akun

**Request:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetTransactionHistory xmlns="http://tempuri.org/">
      <accountNumber>ACC-20240101-7F3A9B2C</accountNumber>
    </GetTransactionHistory>
  </soap:Body>
</soap:Envelope>
```

**Response** (daftar elemen `Transaction`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetTransactionHistoryResponse xmlns="http://tempuri.org/">
      <GetTransactionHistoryResult xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models"
                                   xmlns:i="http://www.w3.org/2001/XMLSchema-instance"
                                   xmlns:a="http://schemas.microsoft.com/2006/08/addressing">
        <q1:Transaction>
          <q1:Amount>400000</q1:Amount>
          <q1:Description>Transfer dari ACC-20240101-7F3A9B2C ke ACC-20240102-1C5D4E7F</q1:Description>
          <q1:FromAccountNumber>ACC-20240101-7F3A9B2C</q1:FromAccountNumber>
          <q1:ToAccountNumber>ACC-20240102-1C5D4E7F</q1:ToAccountNumber>
          <q1:TransactionDate>2024-01-03T08:00:00</q1:TransactionDate>
          <q1:TransactionId>TRX-20240103080000-000001</q1:TransactionId>
          <q1:TransactionType>TRANSFER</q1:TransactionType>
        </q1:Transaction>
        <q1:Transaction>
          <q1:Amount>250000</q1:Amount>
          <q1:Description>Deposit ke akun</q1:Description>
          <q1:FromAccountNumber>EXTERNAL</q1:FromAccountNumber>
          <q1:ToAccountNumber>ACC-20240101-7F3A9B2C</q1:ToAccountNumber>
          <q1:TransactionDate>2024-01-02T07:12:00</q1:TransactionDate>
          <q1:TransactionId>TRX-20240102071200-000001</q1:TransactionId>
          <q1:TransactionType>DEPOSIT</q1:TransactionType>
        </q1:Transaction>
      </GetTransactionHistoryResult>
    </GetTransactionHistoryResponse>
  </soap:Body>
</soap:Envelope>
```

---

#### 8. `GetBalance` — Cek Saldo Akun

**Request:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetBalance xmlns="http://tempuri.org/">
      <accountNumber>ACC-20240101-7F3A9B2C</accountNumber>
    </GetBalance>
  </soap:Body>
</soap:Envelope>
```

**Response** (nilai desimal langsung di `GetBalanceResult`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetBalanceResponse xmlns="http://tempuri.org/">
      <GetBalanceResult>950000</GetBalanceResult>
    </GetBalanceResponse>
  </soap:Body>
</soap:Envelope>
```

---

#### 9. `CloseAccount` — Tutup Akun (syarat: saldo = 0)

**Request:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <CloseAccount xmlns="http://tempuri.org/">
      <accountNumber>ACC-20240101-7F3A9B2C</accountNumber>
    </CloseAccount>
  </soap:Body>
</soap:Envelope>
```

**Response** (berisi `ServiceResponse`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <CloseAccountResponse xmlns="http://tempuri.org/">
      <CloseAccountResult xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models"
                          xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        <q1:Message>Akun ACC-20240101-7F3A9B2C berhasil ditutup</q1:Message>
        <q1:Success>true</q1:Success>
      </CloseAccountResult>
    </CloseAccountResponse>
  </soap:Body>
</soap:Envelope>
```

---

### SOAP Fault — Contoh Respons Error

Operasi yang gagal validasi domain (mis. setor jumlah negatif, atau akun tidak ditemukan)
dikirimkan sebagai **SOAP Fault**:

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <soap:Fault>
      <faultcode>soap:Server</faultcode>
      <faultstring xml:lang="id-ID">Akun dengan nomor ACC-TIDAK-ADA tidak ditemukan</faultstring>
      <detail>
        <ExceptionDetail xmlns="http://schemas.microsoft.com/wsh/2004/07/Exception">
          <Type>System.ServiceModel.FaultException</Type>
          <Message>Akun dengan nomor ACC-TIDAK-ADA tidak ditemukan</Message>
          <StackTrace>Ranch dipotong untuk contoh</StackTrace>
        </ExceptionDetail>
      </detail>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>
```

Klien SOAP (baik di `BankingClient`, maupun pemanggil eksternal) dapat menangkap ini
dengan `try { ... } catch (FaultException ex) { /* ex.Message */ }`.

---

## Referensi REST API

- **Base URL**: `http://localhost:5225/api/BankingApi`
- Dokumentasi interaktif: `http://localhost:5225/swagger`

| Metode | Rute                                    | Deskripsi                    | Kode Sukses |
|--------|-----------------------------------------|------------------------------|-------------|
| POST   | `/accounts`                             | Membuat akun                 | 201         |
| GET    | `/accounts/{accountNumber}`             | Detail akun                  | 200         |
| GET    | `/accounts`                             | Daftar semua akun            | 200         |
| POST   | `/deposit`                              | Setor tunai                  | 200         |
| POST   | `/withdraw`                             | Tarik tunai                  | 200         |
| POST   | `/transfer`                             | Transfer inter-akun          | 200         |
| GET    | `/accounts/{accountNumber}/transactions` | Riwayat transaksi           | 200         |
| GET    | `/accounts/{accountNumber}/balance`     | Saldo akun                   | 200         |
| DELETE | `/accounts/{accountNumber}`             | Tutup akun                   | 200         |

### Contoh: Membuat Akun (REST)

```bash
curl -X POST http://localhost:5225/api/BankingApi/accounts \
  -H "Content-Type: application/json" \
  -d '{ "accountHolderName": "Budi Santoso", "accountType": "Giro", "initialBalance": 1000000 }'
```

Respons `201 Created` berisi objek `Account`.

---

## Routing MVC UI

| Metode | Rute (`/Banking/...`)                   | Deskripsi                |
|--------|-------------------------------------------|--------------------------|
| GET    | `/Banking`                              | Beranda / menu fitur     |
| GET    | `/Banking/CreateAccount`                | Form buat akun           |
| POST   | `/Banking/CreateAccount`                | Submit buat akun         |
| GET    | `/Banking/AccountDetails?accountNumber=ACC-...` | Detail akun      |
| GET    | `/Banking/AllAccounts`                  | Daftar semua akun        |
| GET    | `/Banking/Deposit`                     | Form setor              |
| POST   | `/Banking/Deposit`                     | Submit setor            |
| GET    | `/Banking/Withdraw`                    | Form tarik              |
| POST   | `/Banking/Withdraw`                    | Submit tarik            |
| GET    | `/Banking/Transfer`                    | Form transfer           |
| POST   | `/Banking/Transfer`                    | Submit transfer         |
| GET    | `/Banking/Transactions?accountNumber=ACC-...` | Riwayat transaksi |
| GET    | `/Banking/CheckBalance?accountNumber=ACC-...` | Cek saldo         |
| GET    | `/Banking/CloseAccount`                | Form tutup akun          |
| POST   | `/Banking/CloseAccount`                | Submit tutup akun        |

---

## Model / Data Contract

Semua model berada di `Models/BankingModels.cs` dan didekorasi dengan `[DataContract]`
serta `[DataMember]` agar dapat disertakan dalam pesan SOAP.

| Kelas                 | Properti                                          | Digunakan oleh    |
|-----------------------|---------------------------------------------------|--------------------|
| `Account`             | AccountNumber, AccountHolderName, Balance, AccountType, CreatedDate | semua operasi |
| `Transaction`         | TransactionId, FromAccountNumber, ToAccountNumber, Amount, TransactionType, TransactionDate, Description | riwayat & mutasi |
| `CreateAccountRequest`| AccountHolderName, InitialBalance, AccountType    | CreateAccount     |
| `DepositRequest`      | AccountNumber, Amount                             | Deposit           |
| `WithdrawRequest`     | AccountNumber, Amount                             | Withdraw          |
| `TransferRequest`     | FromAccountNumber, ToAccountNumber, Amount        | Transfer          |
| `ServiceResponse`     | Success, Message                                  | Transfer, CloseAccount |

---

## Cara Menjalankan

### Prasyarat

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) terpasang
- (Opsional) Postman / `curl` untuk menguji REST & SOAP

### Langkah-langkah

```bash
# 1. Restore dependensi
dotnet restore

# 2. Build
dotnet build

# 3. Jalankan (development; otomatis membuka swagger)
dotnet run
```

Aplikasi akan tersedia di:

| Layanan        | URL                                              |
|----------------|--------------------------------------------------|
| Swagger UI      | http://localhost:5225/swagger                    |
| SOAP endpoint   | http://localhost:5225/BankingService.svc         |
| REST API        | http://localhost:5225/api/BankingApi             |
| Info endpoint   | http://localhost:5225/api/info                   |
| MVC UI          | http://localhost:5225/Banking                    |

### Menguji SOAP dengan `curl`

```bash
curl -H "Content-Type: text/xml; charset=utf-8" \
     -H "SOAPAction: \"http://tempuri.org/IBankingService/GetAllAccounts\"" \
     -d @- \
     http://localhost:5225/BankingService.svc <<'EOF'
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetAllAccounts xmlns="http://tempuri.org/" />
  </soap:Body>
</soap:Envelope>
EOF
```

---

## Konfigurasi

### `appsettings.json`

```json
{
  "BankingServiceUrl": "http://localhost:5225/BankingService.svc",
  "Logging": { ... },
  "AllowedHosts": "*"
}
```

- **`BankingServiceUrl`**: URL SOAP service yang dipakai klien MVC & REST API.
  Pada mode development, klien akan menghubungi layanan dalam proses yang sama.
- **`ASPNETCORE_ENVIRONMENT`**: Diatur ke `Development` pada profil `http`/`https`
  lewat `launchSettings.json`, yang mengaktifkan Swagger dan XML-comments pada dok
  API.

---

## CI/CD

Pipeline GitHub Actions (`.github/workflows/dotnet.yml`) berjalan pada setiap *push*
dan *pull request* ke cabang `main`:

| Langkah          | Perintah                          |
|------------------|-----------------------------------|
| Checkout         | `actions/checkout@v4`             |
| Setup .NET       | `actions/setup-dotnet@v4` (8.0.x) |
| Restore          | `dotnet restore`                  |
| Build            | `dotnet build --no-restore`       |
| Test             | `dotnet test --no-build`          |

> Lingkungan CI berjalan pada `ubuntu-latest`. Perhatikan aplikasi ini **belum memiliki
> proyek unit test** yang termasuk — `dotnet test` akan selesai dengan cepat karena
> tidak ada proyek test yang direferensikan.

---

## Batasan & Catatan

1. **Penyimpanan in-memory**: Akun dan transaksi disimpan dalam `static List` di
   `BankingService` — semua data **hilang saat aplikasi dimatikan atau *restart***.
   Tidak cocok untuk produksi; gunakan database (mis. EF Core + SQL Server) untuk data
   yang persisten.
2. **Thread-safety**: Penggunaan koleksi statis bersama tidak dilengkapi lock
   (`lock`/`ConcurrentDictionary`). Pada beban serentak tinggi dapat terjadi race
   condition.
3. **BankingClient (SOAP klien) manual**: Klien tidak memakai *service reference*
   hasil `svcutil` atau `dotnet-svcutil`, melainkan mem-build *envelope* SOAP via
   string interpolation dan mem-parsing balasan pakai `XDocument`/`XElement`. Hal ini
   menghindari kebergantungan ekstra namun membutuhkan pemeliharaan tangan jika
   kontrak berubah.
4. **Keamanan**: Tidak ada otentikasi, otorisasi, atau enkripsi (HTTPS/TLS) yang
   diterapkan secara eksplisit di kode. SOAP juga tidak dilengkapi *security binding*.
5. **FaultException**: Semua error validasi domain dikirimkan ke klien sebagai
   `FaultException` (SOAP fault) — klien MVC/REST menangkapnya lewat `catch`.
6. **File `.http`**: Berkas `AplikasiWebMethodSOAP.http` berisi permintaan contoh
   yang menunjuk ke `/weatherforecast` — ini sisa template *boilerplate* ASP.NET Core
   dan tidak digunakan oleh aplikasi ini.
