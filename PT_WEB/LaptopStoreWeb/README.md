# Laptop Store Web

Simple ASP.NET Core MVC project for a laptop shopping website.

## Main Features

- Customer register, login, logout
- View products with pagination
- Search by product name
- Filter by category
- Sort by price
- Add to cart, update quantity, remove from cart
- Checkout and create order
- Admin login
- Admin manage products
- Admin manage order status
- Admin revenue dashboard with 7-day chart

## Tech Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQL Server
- Bootstrap 5
- Chart.js

## Default Accounts

- Admin
  - Email: admin@laptopstore.com
  - Password: Admin@123
- Customer 1
  - Email: customer1@gmail.com
  - Password: Customer@123
- Customer 2
  - Email: customer2@gmail.com
  - Password: Customer@123

## Database Setup

The project uses SQL Server.

Default connection string in `appsettings.json`:

```json
"DefaultConnection": "Server=127.0.0.1,1433;Database=LaptopStoreDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true"
```

If you use Docker on macOS, one simple way is:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=root@123" -p 1433:1433 --name laptopstore-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

After SQL Server is running, start the project:

```bash
dotnet run
```

## Notes

- The app creates database tables automatically on startup by using `EnsureCreated()`.
- Seed data includes 1 admin, 2 customers, 4 categories, and 30 products.
- Product images use internet random images via `https://picsum.photos` with stable seed IDs.