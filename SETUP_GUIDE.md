
## Step 1: Verify Folder Structure and SQL Server is Running

### Folder Structure

```
PT_PHAN-MEM-UNG-DUNG/
├── PT_WEB_BACKEND/
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── LaptopStoreDb_seed.sql
│   ├── Program.cs
│   ├── PT_WEB_BACKEND.csproj
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   └── Views/
├── PT_WINFORM/
│   ├── Program.cs
│   ├── PT_WINFORM.csproj
│   ├── UI/
│   │   ├── MainForm.cs
│   │   └── MainForm.Designer.cs
│   ├── Services/
│   ├── Models/
│   └── Business/


```

* Before importing the database, ensure SQL Server is running.

---

## Step 2: Import Database & Seed Data

### 2.1 Using sqlcmd (Command Line)

1. Open PowerShell or Command Prompt in the PT_WEB_BACKEND folder:
   ```powershell
   cd {PROJECT_ROOT}\APP\PT_WEB_BACKEND
   ```

2. Run the seed script to create database and populate test data:
   ```powershell
   sqlcmd -S localhost,1433 -U sa -P "sa@123456" -C -i "LaptopStoreDb_seed.sql"
   ```

   **Expected Output:**
   - Database `LaptopStoreDb` created
   - Tables created: UserAccounts, Categories, Products, Orders, OrderItems
   - Seed data inserted: 1 admin user, 2 customer users, 4 categories, 30 products, 7 sample orders

---

## Step 3: Configure PT_WEB_BACKEND Connection String

File: `{PROJECT_ROOT}/APP/PT_WEB_BACKEND/appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=LaptopStoreDb;User Id=sa;Password=sa@123456;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true"
}
```

**Parameters:**
- `Server` — SQL Server instance (e.g., `localhost,1433` or `.\SQLEXPRESS`)
- `Database` — Database name (`LaptopStoreDb`)
- `User Id` — SQL login (`sa`)
- `Password` — Your SQL Server sa password
- `TrustServerCertificate=True` — Skip SSL verification (dev-only)

---

## Step 4: Run PT_WEB_BACKEND (ASP.NET Backend)

### 4.1 Navigate to PT_WEB_BACKEND Directory

```powershell
cd {PROJECT_ROOT}\APP\PT_WEB_BACKEND
```

### 4.2 Restore Dependencies & Run

```powershell
dotnet run
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5226
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to exit.
```

---


## Step 5: Run PT_WINFORM (Desktop Client)

### 5.1 Open New PowerShell Window (Keep PT_WEB_BACKEND Running)

```powershell
cd {PROJECT_ROOT}\APP\PT_WINFORM
```

### 5.2 Restore Dependencies & Run

```powershell
dotnet run
```

**Expected Output:**
```
Application started.
```


### 5.3 Test the App

**Login** with test account:
   - Email: `customer1@gmail.com`
   - Password: `Customer@123`


---


## Default Test Accounts

### Admin Account
- **Email:** `admin@laptopstore.com`
- **Password:** `Admin@123`
- **Role:** Admin (can manage products, orders, view dashboard)

### Customer Accounts
- **Email:** `customer1@gmail.com`
- **Password:** `Customer@123`
- **Role:** Customer

- **Email:** `customer2@gmail.com`
- **Password:** `Customer@123`
- **Role:** Customer


---