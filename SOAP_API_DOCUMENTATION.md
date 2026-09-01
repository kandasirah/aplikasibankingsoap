# BankingService.svc - SOAP API Documentation

Dokumentasi lengkap untuk SOAP Web Service BankingService yang menggunakan CoreWCF.

## Informasi Umum

| Property | Value |
|----------|-------|
| **Endpoint** | `/BankingService.svc` |
| **Protocol** | SOAP 1.1 over HTTP |
| **Binding** | BasicHttpBinding |
| **Namespace** | `http://tempuri.org/` |
| **Data Namespace** | `http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models` |

---

## Cara Request ke SOAP Service

### Format SOAP Envelope

Semua request menggunakan format SOAP XML:

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <OperationName xmlns="http://tempuri.org/">
      <!-- Request parameters -->
    </OperationName>
  </soap:Body>
</soap:Envelope>
```

### HTTP Headers

| Header | Value |
|--------|-------|
| `Content-Type` | `text/xml` |
| `SOAPAction` | `"http://tempuri.org/IBankingService/{OperationName}"` |

---

## Operations

### 1. CreateAccount

Membuat akun baru di sistem.

#### Request

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <CreateAccount xmlns="http://tempuri.org/">
      <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <q1:AccountHolderName>John Doe</q1:AccountHolderName>
        <q1:AccountType>SAVINGS</q1:AccountType>
        <q1:InitialBalance>1000000</q1:InitialBalance>
      </request>
    </CreateAccount>
  </soap:Body>
</soap:Envelope>
```

#### Response (Success)

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <CreateAccountResponse xmlns="http://tempuri.org/">
      <CreateAccountResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <a:AccountNumber>ACC-20260901-A1B2C3D4</a:AccountNumber>
        <a:AccountHolderName>John Doe</a:AccountHolderName>
        <a:Balance>1000000</a:Balance>
        <a:AccountType>SAVINGS</a:AccountType>
        <a:CreatedDate>2026-09-01T10:30:00Z</a:CreatedDate>
      </CreateAccountResult>
    </CreateAccountResponse>
  </soap:Body>
</soap:Envelope>
```

#### Error Response

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <soap:Fault>
      <faultcode>soap:Server</faultcode>
      <faultstring>Nama pemegang akun tidak boleh kosong</faultstring>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>
```

---

### 2. GetAccount

Mendapatkan detail akun berdasarkan nomor akun.

#### Request

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetAccount xmlns="http://tempuri.org/">
      <accountNumber>ACC-20260901-A1B2C3D4</accountNumber>
    </GetAccount>
  </soap:Body>
</soap:Envelope>
```

#### Response (Success)

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetAccountResponse xmlns="http://tempuri.org/">
      <GetAccountResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <a:AccountNumber>ACC-20260901-A1B2C3D4</a:AccountNumber>
        <a:AccountHolderName>John Doe</a:AccountHolderName>
        <a:Balance>1000000</a:Balance>
        <a:AccountType>SAVINGS</a:AccountType>
        <a:CreatedDate>2026-09-01T10:30:00Z</a:CreatedDate>
      </GetAccountResult>
    </GetAccountResponse>
  </soap:Body>
</soap:Envelope>
```

#### Response (Not Found)

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetAccountResponse xmlns="http://tempuri.org/">
      <GetAccountResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models" />
    </GetAccountResponse>
  </soap:Body>
</soap:Envelope>
```

---

### 3. GetAllAccounts

Mendapatkan semua daftar akun yang terdaftar.

#### Request

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetAllAccounts xmlns="http://tempuri.org/">
    </GetAllAccounts>
  </soap:Body>
</soap:Envelope>
```

#### Response

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetAllAccountsResponse xmlns="http://tempuri.org/">
      <GetAllAccountsResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <a:Account>
          <a:AccountNumber>ACC-20260901-A1B2C3D4</a:AccountNumber>
          <a:AccountHolderName>John Doe</a:AccountHolderName>
          <a:Balance>1000000</a:Balance>
          <a:AccountType>SAVINGS</a:AccountType>
          <a:CreatedDate>2026-09-01T10:30:00Z</a:CreatedDate>
        </a:Account>
        <a:Account>
          <a:AccountNumber>ACC-20260901-E5F6G7H8</a:AccountNumber>
          <a:AccountHolderName>Jane Smith</a:AccountHolderName>
          <a:Balance>2500000</a:Balance>
          <a:AccountType>CHECKING</a:AccountType>
          <a:CreatedDate>2026-09-01T11:00:00Z</a:CreatedDate>
        </a:Account>
      </GetAllAccountsResult>
    </GetAllAccountsResponse>
  </soap:Body>
</soap:Envelope>
```

---

### 4. Deposit

Melakukan deposit ke akun.

#### Request

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <Deposit xmlns="http://tempuri.org/">
      <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <q1:AccountNumber>ACC-20260901-A1B2C3D4</q1:AccountNumber>
        <q1:Amount>500000</q1:Amount>
      </request>
    </Deposit>
  </soap:Body>
</soap:Envelope>
```

#### Response (Success)

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <DepositResponse xmlns="http://tempuri.org/">
      <DepositResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <a:AccountNumber>ACC-20260901-A1B2C3D4</a:AccountNumber>
        <a:AccountHolderName>John Doe</a:AccountHolderName>
        <a:Balance>1500000</a:Balance>
        <a:AccountType>SAVINGS</a:AccountType>
        <a:CreatedDate>2026-09-01T10:30:00Z</a:CreatedDate>
      </DepositResult>
    </DepositResponse>
  </soap:Body>
</soap:Envelope>
```

#### Error Response

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <soap:Fault>
      <faultcode>soap:Server</faultcode>
      <faultstring>Akun dengan nomor ACC-20260901-XXXX tidak ditemukan</faultstring>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>
```

---

### 5. Withdraw

Melakukan penarikan dari akun.

#### Request

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <Withdraw xmlns="http://tempuri.org/">
      <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <q1:AccountNumber>ACC-20260901-A1B2C3D4</q1:AccountNumber>
        <q1:Amount>200000</q1:Amount>
      </request>
    </Withdraw>
  </soap:Body>
</soap:Envelope>
```

#### Response (Success)

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <WithdrawResponse xmlns="http://tempuri.org/">
      <WithdrawResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <a:AccountNumber>ACC-20260901-A1B2C3D4</a:AccountNumber>
        <a:AccountHolderName>John Doe</a:AccountHolderName>
        <a:Balance>800000</a:Balance>
        <a:AccountType>SAVINGS</a:AccountType>
        <a:CreatedDate>2026-09-01T10:30:00Z</a:CreatedDate>
      </WithdrawResult>
    </WithdrawResponse>
  </soap:Body>
</soap:Envelope>
```

#### Error Response (Saldo Tidak Cukup)

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <soap:Fault>
      <faultcode>soap:Server</faultcode>
      <faultstring>Saldo tidak mencukupi</faultstring>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>
```

---

### 6. Transfer

Melakukan transfer antar akun.

#### Request

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <Transfer xmlns="http://tempuri.org/">
      <request xmlns:q1="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <q1:Amount>300000</q1:Amount>
        <q1:FromAccountNumber>ACC-20260901-A1B2C3D4</q1:FromAccountNumber>
        <q1:ToAccountNumber>ACC-20260901-E5F6G7H8</q1:ToAccountNumber>
      </request>
    </Transfer>
  </soap:Body>
</soap:Envelope>
```

#### Response (Success)

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <TransferResponse xmlns="http://tempuri.org/">
      <TransferResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <a:Success>true</a:Success>
        <a:Message>Transfer sebesar Rp300.000,00 berhasil dari ACC-20260901-A1B2C3D4 ke ACC-20260901-E5F6G7H8</a:Message>
      </TransferResult>
    </TransferResponse>
  </soap:Body>
</soap:Envelope>
```

#### Error Response

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <soap:Fault>
      <faultcode>soap:Server</faultcode>
      <faultstring>Akun sumber dan tujuan tidak boleh sama</faultstring>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>
```

---

### 7. GetTransactionHistory

Mendapatkan riwayat transaksi akun.

#### Request

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetTransactionHistory xmlns="http://tempuri.org/">
      <accountNumber>ACC-20260901-A1B2C3D4</accountNumber>
    </GetTransactionHistory>
  </soap:Body>
</soap:Envelope>
```

#### Response

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetTransactionHistoryResponse xmlns="http://tempuri.org/">
      <GetTransactionHistoryResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <a:Transaction>
          <a:TransactionId>TRX-20260901103500-000001</a:TransactionId>
          <a:FromAccountNumber>EXTERNAL</a:FromAccountNumber>
          <a:ToAccountNumber>ACC-20260901-A1B2C3D4</a:ToAccountNumber>
          <a:Amount>500000</a:Amount>
          <a:TransactionType>DEPOSIT</a:TransactionType>
          <a:TransactionDate>2026-09-01T10:35:00Z</a:TransactionDate>
          <a:Description>Deposit ke akun</a:Description>
        </a:Transaction>
        <a:Transaction>
          <a:TransactionId>TRX-20260901103000-000001</a:TransactionId>
          <a:FromAccountNumber>SYSTEM</a:FromAccountNumber>
          <a:ToAccountNumber>ACC-20260901-A1B2C3D4</a:ToAccountNumber>
          <a:Amount>1000000</a:Amount>
          <a:TransactionType>DEPOSIT</a:TransactionType>
          <a:TransactionDate>2026-09-01T10:30:00Z</a:TransactionDate>
          <a:Description>Setoran awal pembukaan akun</a:Description>
        </a:Transaction>
      </GetTransactionHistoryResult>
    </GetTransactionHistoryResponse>
  </soap:Body>
</soap:Envelope>
```

---

### 8. GetBalance

Memeriksa saldo akun.

#### Request

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetBalance xmlns="http://tempuri.org/">
      <accountNumber>ACC-20260901-A1B2C3D4</accountNumber>
    </GetBalance>
  </soap:Body>
</soap:Envelope>
```

#### Response

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetBalanceResponse xmlns="http://tempuri.org/">
      <GetBalanceResult>1000000</GetBalanceResult>
    </GetBalanceResponse>
  </soap:Body>
</soap:Envelope>
```

#### Error Response

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <soap:Fault>
      <faultcode>soap:Server</faultcode>
      <faultstring>Akun dengan nomor ACC-20260901-XXXX tidak ditemukan</faultstring>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>
```

---

### 9. CloseAccount

Menutup akun (saldo harus 0).

#### Request

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <CloseAccount xmlns="http://tempuri.org/">
      <accountNumber>ACC-20260901-A1B2C3D4</accountNumber>
    </CloseAccount>
  </soap:Body>
</soap:Envelope>
```

#### Response (Success)

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <CloseAccountResponse xmlns="http://tempuri.org/">
      <CloseAccountResult xmlns:a="http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models">
        <a:Success>true</a:Success>
        <a:Message>Akun ACC-20260901-A1B2C3D4 berhasil ditutup</a:Message>
      </CloseAccountResult>
    </CloseAccountResponse>
  </soap:Body>
</soap:Envelope>
```

#### Error Response (Saldo Masih Ada)

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <soap:Fault>
      <faultcode>soap:Server</faultcode>
      <faultstring>Akun tidak dapat ditutup karena masih memiliki saldo. Silakan tarik saldo terlebih dahulu.</faultstring>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>
```

---

## Data Models

### Account

| Field | Type | Description |
|-------|------|-------------|
| `AccountNumber` | string | Nomor akun (format: ACC-YYYYMMDD-XXXXXXXX) |
| `AccountHolderName` | string | Nama pemegang akun |
| `Balance` | decimal | Saldo akun |
| `AccountType` | string | Tipe akun (SAVINGS/CHECKING) |
| `CreatedDate` | DateTime | Tanggal pembuatan akun |

### Transaction

| Field | Type | Description |
|-------|------|-------------|
| `TransactionId` | string | ID transaksi (format: TRX-YYYYMMDDHHMMSS-XXXXXX) |
| `FromAccountNumber` | string | Nomor akun sumber |
| `ToAccountNumber` | string | Nomor akun tujuan |
| `Amount` | decimal | Jumlah transaksi |
| `TransactionType` | string | Tipe transaksi (DEPOSIT/WITHDRAWAL/TRANSFER) |
| `TransactionDate` | DateTime | Tanggal transaksi |
| `Description` | string | Deskripsi transaksi |

### CreateAccountRequest

| Field | Type | Description |
|-------|------|-------------|
| `AccountHolderName` | string | Nama pemegang akun |
| `InitialBalance` | decimal | Saldo awal |
| `AccountType` | string | Tipe akun |

### DepositRequest / WithdrawRequest

| Field | Type | Description |
|-------|------|-------------|
| `AccountNumber` | string | Nomor akun |
| `Amount` | decimal | Jumlah |

### TransferRequest

| Field | Type | Description |
|-------|------|-------------|
| `FromAccountNumber` | string | Akun sumber |
| `ToAccountNumber` | string | Akun tujuan |
| `Amount` | decimal | Jumlah transfer |

### ServiceResponse

| Field | Type | Description |
|-------|------|-------------|
| `Success` | boolean | Status berhasil/gagal |
| `Message` | string | Pesan response |

---

## Contoh Menggunakan cURL

### CreateAccount
```bash
curl -X POST http://localhost:5225/BankingService.svc \
  -H "Content-Type: text/xml" \
  -H "SOAPAction: \"http://tempuri.org/IBankingService/CreateAccount\"" \
  -d '<?xml version=\"1.0\" encoding=\"utf-8\"?>
<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">
  <soap:Body>
    <CreateAccount xmlns=\"http://tempuri.org/\">
      <request xmlns:q1=\"http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models\">
        <q1:AccountHolderName>John Doe</q1:AccountHolderName>
        <q1:AccountType>SAVINGS</q1:AccountType>
        <q1:InitialBalance>1000000</q1:InitialBalance>
      </request>
    </CreateAccount>
  </soap:Body>
</soap:Envelope>'
```

### GetAccount
```bash
curl -X POST http://localhost:5225/BankingService.svc \
  -H "Content-Type: text/xml" \
  -H "SOAPAction: \"http://tempuri.org/IBankingService/GetAccount\"" \
  -d '<?xml version=\"1.0\" encoding=\"utf-8\"?>
<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">
  <soap:Body>
    <GetAccount xmlns=\"http://tempuri.org/\">
      <accountNumber>ACC-20260901-A1B2C3D4</accountNumber>
    </GetAccount>
  </soap:Body>
</soap:Envelope>'
```

### Deposit
```bash
curl -X POST http://localhost:5225/BankingService.svc \
  -H "Content-Type: text/xml" \
  -H "SOAPAction: \"http://tempuri.org/IBankingService/Deposit\"" \
  -d '<?xml version=\"1.0\" encoding=\"utf-8\"?>
<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">
  <soap:Body>
    <Deposit xmlns=\"http://tempuri.org/\">
      <request xmlns:q1=\"http://schemas.datacontract.org/2004/07/AplikasiWebMethodSOAP.Models\">
        <q1:AccountNumber>ACC-20260901-A1B2C3D4</q1:AccountNumber>
        <q1:Amount>500000</q1:Amount>
      </request>
    </Deposit>
  </soap:Body>
</soap:Envelope>'
```

---

## WSDL

Untuk mendapatkan WSDL (Web Services Description Language), akses:
```
http://localhost:5225/BankingService.svc?wsdl
```

WSDL berisi deskripsi lengkap service, operations, dan data contracts yang digunakan.
