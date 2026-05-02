# Technical Specification - Laptop E-commerce Website

## 1. Source Summary

This document is derived from the assignment brief in `PT_WEB_TOPIC.docx`.

Original assignment intent:
- Build an e-commerce website for selling laptops.
- Required technology: .NET Core and SQL Server.
- Delivery deadline: 15/05/2026.
- Required submission artifacts: project zip, SQL script, and demo video.

## 2. Project Goal

Build a web-based laptop store with two main roles:
- Customer: browse products, search, sort, manage cart, and place orders.
- Admin: manage products, manage orders, and view revenue statistics.

The system must support a realistic product catalog, basic user account flows, order lifecycle handling, and reporting for daily, monthly, and yearly revenue.

## 3. Scope

### In Scope

- Customer registration, login, logout.
- Product listing with pagination.
- Product search by name and category.
- Product sorting by price ascending and descending.
- Shopping cart management.
- Checkout flow for authenticated customers.
- Admin login/logout.
- Product CRUD for admin.
- Order viewing and order status update for admin.
- Revenue dashboard with totals and 7-day bar chart.

### Out of Scope

- Online payment gateway integration.
- Real shipping provider integration.
- Multi-vendor support.
- Inventory sync with external systems.
- Advanced promotions, coupons, or loyalty programs.
- Real-time chat or recommendation engine.

## 4. Business Requirements

### 4.1 Data Initialization Requirements

- Seed 1 admin account.
- Seed at least 2 customer accounts.
- Seed at least 30 laptop products.
- Each product must include at least 1 image.

### 4.2 Customer Functional Requirements

1. User account
- Register a new customer account.
- Login with existing credentials.
- Logout securely.

2. Product browsing
- View product list with pagination.
- View product details.
- Search products by name.
- Filter or search products by category.
- Sort products by price ascending.
- Sort products by price descending.

3. Cart and order
- Add product to cart.
- Update cart item quantity.
- Remove item from cart.
- View cart summary.
- Checkout only when logged in.
- Create order from cart.

### 4.3 Admin Functional Requirements

1. Authentication
- Login.
- Logout.

2. Product management
- View product list.
- Add new product.
- Edit existing product.
- Delete product.

3. Order management
- View order list.
- View order details.
- Update order status.

4. Revenue reporting
- Select a specific date.
- Display total revenue for the selected day.
- Display total revenue for the selected month.
- Display total revenue for the selected year.
- Display a bar chart for the previous 7 days including the selected day.

## 5. Non-Functional Requirements

### 5.1 Usability

- UI must be simple and easy to navigate for both customer and admin users.
- Product browsing and cart actions should be achievable in a few clicks.
- Pagination, search, and sorting must remain consistent on desktop and mobile layouts.

### 5.2 Performance

- Product listing pages should load efficiently with server-side pagination.
- Search and filtering should respond quickly on a catalog of at least 30 products.
- Revenue report queries should complete in acceptable time for small to medium academic datasets.

### 5.3 Security

- Passwords must be hashed, not stored in plain text.
- Role-based authorization must protect admin pages and APIs.
- Checkout must require authentication.
- Input validation must be enforced on both client and server.

### 5.4 Maintainability

- Project should follow a layered structure.
- Business logic should be separated from controllers and views.
- Database schema and seed data should be reproducible from SQL script or migrations.

## 6. Recommended Technical Stack

The assignment requires .NET Core and SQL Server. The following stack fits that requirement while staying practical for implementation.

### 6.1 Backend

- Framework: ASP.NET Core 8 MVC
- Language: C#
- ORM: Entity Framework Core 8
- Authentication: ASP.NET Core Identity or cookie authentication with role support
- Validation: Data Annotations and server-side model validation
- Mapping: Manual mapping or AutoMapper if preferred

### 6.2 Frontend

- Server-rendered Razor Views
- HTML5, CSS3, JavaScript
- UI framework: Bootstrap 5
- Charts: Chart.js for revenue bar chart

### 6.3 Database

- SQL Server
- EF Core migrations for schema evolution
- Optional SQL seed script for submission requirement

### 6.4 Tooling

- IDE: Visual Studio 2022 or VS Code with C# Dev Kit
- Version control: Git
- Package manager: NuGet
- API and browser testing: Postman and browser developer tools

## 7. Suggested Architecture

### 7.1 Application Style

Use a layered monolith:
- Presentation layer: MVC controllers and Razor views.
- Application layer: business services for products, cart, orders, reporting.
- Data access layer: EF Core DbContext, repositories if needed.
- Database layer: SQL Server.

This is the most efficient architecture for a course project because it is easier to build, demo, and deploy than a split frontend-backend system.

### 7.2 Suggested Project Structure

```text
PT_WEB/
  Controllers/
    HomeController.cs
    ProductsController.cs
    CartController.cs
    OrdersController.cs
    AccountController.cs
    Admin/
      DashboardController.cs
      ProductsController.cs
      OrdersController.cs
  Models/
    Entities/
    ViewModels/
  Services/
    Interfaces/
    Implementations/
  Data/
    ApplicationDbContext.cs
    SeedData.cs
    Configurations/
  Views/
  wwwroot/
    css/
    js/
    images/
  Migrations/
```

## 8. Core Modules

### 8.1 Authentication Module

- Customer registration and login.
- Admin login.
- Role-based access control.

### 8.2 Catalog Module

- Product listing.
- Product details.
- Search, category filtering, sorting, pagination.

### 8.3 Cart Module

- Session-based or database-backed shopping cart.
- Add, update, remove items.
- Cart total calculation.

### 8.4 Order Module

- Create order from cart.
- Store order items snapshot.
- Track order status.

### 8.5 Admin Module

- Product CRUD.
- Order review and status management.
- Revenue dashboard.

## 9. Database Design

### 9.1 Main Entities

1. User
- UserId
- FullName
- Email
- PasswordHash
- Role
- CreatedAt
- IsActive

2. Category
- CategoryId
- CategoryName
- Description

3. Product
- ProductId
- CategoryId
- ProductName
- Brand
- Description
- Price
- StockQuantity
- ImageUrl
- IsActive
- CreatedAt
- UpdatedAt

4. CartItem
- CartItemId
- UserId
- ProductId
- Quantity
- UnitPrice

5. Order
- OrderId
- UserId
- OrderDate
- TotalAmount
- Status

6. OrderItem
- OrderItemId
- OrderId
- ProductId
- ProductName
- UnitPrice
- Quantity
- LineTotal

### 9.2 Relationships

- One Category has many Products.
- One User has many Orders.
- One Order has many OrderItems.
- One Product can appear in many CartItems and OrderItems.

### 9.3 Order Status Values

Use exactly the statuses required by the brief:
- New
- Shipped
- Paid

If the UI is in Vietnamese, the labels can be:
- Mới
- Đã vận chuyển
- Đã thanh toán

## 10. Key Workflows

### 10.1 Customer Purchase Flow

1. Customer visits home page.
2. Customer browses or searches products.
3. Customer views product details.
4. Customer adds item to cart.
5. Customer updates quantities if needed.
6. Customer logs in if not authenticated.
7. Customer confirms checkout.
8. System creates order and order items.
9. System clears the cart.

### 10.2 Admin Product Management Flow

1. Admin logs in.
2. Admin opens product management page.
3. Admin adds, edits, or deletes product.
4. System validates input and persists changes.

### 10.3 Revenue Reporting Flow

1. Admin selects a date.
2. System calculates revenue for day, month, and year.
3. System retrieves revenue totals for the last 7 days.
4. System renders bar chart using Chart.js.

## 11. Reporting Logic

Revenue should be calculated from completed or paid orders according to the implemented business rule. To keep behavior consistent for the demo, define one rule clearly and apply it everywhere.

Recommended rule:
- Count revenue from orders with status `Paid`.

Example calculations:
- Daily revenue: sum of paid orders on selected date.
- Monthly revenue: sum of paid orders in selected month and year.
- Yearly revenue: sum of paid orders in selected year.
- Seven-day chart: sum revenue per day from selected date going back 6 more days.

## 12. UI Pages

### Customer Pages

- Home page
- Product listing page
- Product detail page
- Register page
- Login page
- Cart page
- Checkout page
- Order confirmation page

### Admin Pages

- Admin login page
- Admin dashboard page
- Product management page
- Product create/edit page
- Order management page
- Revenue report page

## 13. Validation Rules

- Email must be unique.
- Password must meet minimum length requirement.
- Product name is required.
- Category is required.
- Price must be greater than 0.
- Stock quantity must be 0 or greater.
- Cart quantity must be greater than 0.
- Checkout cannot proceed with empty cart.

## 14. Suggested Implementation Decisions

### Recommended Approach

- Use ASP.NET Core MVC instead of SPA architecture.
- Use EF Core Code First for fast schema management.
- Use SQL seed data or EF seeding for demo data.
- Store product image paths in database and image files in `wwwroot/images/products`.
- Use cookie-based authentication with roles.

### Why This Stack

- Matches the assignment requirement exactly.
- Easier to finish before the deadline.
- Simpler to demo than a separate API and frontend.
- Straightforward deployment on local IIS Express or Kestrel.

## 15. Minimum Deliverables

1. Source code zip file.
2. SQL file for database creation and optionally seed data.
3. Demo video showing:
- Student introduction.
- Customer features.
- Admin features.
- Spoken explanation during demo.

## 16. Acceptance Criteria

The project is acceptable when:
- The application runs successfully with .NET Core and SQL Server.
- Admin can log in and manage products.
- Customer can register, log in, browse products, and place an order.
- Search, sorting, and pagination work correctly.
- Cart operations work correctly.
- Order status updates work correctly.
- Revenue totals and 7-day chart display correctly.
- Seed data includes 1 admin, 2 customers, and at least 30 products.
- Submission package includes source zip, SQL file, and demo video.

## 17. Recommended Build Plan

### Phase 1
- Create solution and configure SQL Server connection.
- Build database models and migrations.
- Seed admin, customer accounts, categories, and products.

### Phase 2
- Implement authentication and authorization.
- Implement customer catalog pages.
- Implement cart and checkout flow.

### Phase 3
- Implement admin product management.
- Implement order management.
- Implement revenue reporting and chart.

### Phase 4
- Test end-to-end flows.
- Prepare SQL script.
- Record demo video.
- Package final submission.

## 18. Risks and Clarifications

### Risks

- Unclear revenue rule can cause inconsistent report results.
- Late changes in product schema can slow progress.
- Image upload handling can become unstable if not standardized early.

### Clarifications to Lock Early

- Whether admins are stored in the same user table as customers.
- Whether checkout collects shipping address and phone number.
- Whether revenue counts only paid orders or also shipped orders.
- Whether product categories are fixed or managed by admin.

## 19. Final Recommendation

Build the project as an ASP.NET Core MVC application with EF Core and SQL Server. This is the most direct way to satisfy the assignment requirements while keeping implementation effort controlled. Prioritize clean CRUD flows, stable authentication, correct cart behavior, and accurate revenue reporting over extra features.