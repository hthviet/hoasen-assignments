# ASP.NET Core MVC Architecture

## Project Structure

The web application is implemented as a layered ASP.NET Core MVC monolith inside `LaptopStoreWeb`:

```
PT_WEB/
└── LaptopStoreWeb/
    ├── Controllers/                 # Presentation endpoints (MVC + API)
    │   ├── HomeController.cs
    │   ├── ProductsController.cs
    │   ├── CartController.cs
    │   ├── OrdersController.cs
    │   ├── AccountController.cs
    │   ├── AdminDashboardController.cs
    │   ├── AdminProductsController.cs
    │   ├── AdminOrdersController.cs
    │   └── Api/
    │       ├── AuthApiController.cs
    │       ├── ProductsApiController.cs
    │       └── OrdersApiController.cs
    │
    ├── Models/                      # Domain models and view models
    │   ├── Category.cs
    │   ├── Product.cs
    │   ├── Order.cs
    │   ├── OrderItem.cs
    │   ├── UserAccount.cs
    │   ├── UserRole.cs
    │   ├── OrderStatus.cs
    │   └── ViewModels/
    │
    ├── Data/                        # EF Core data access and seeding
    │   ├── ApplicationDbContext.cs
    │   └── SeedData.cs
    │
    ├── Extensions/                  # Reusable web helpers
    │   ├── ClaimsPrincipalExtensions.cs
    │   └── SessionExtensions.cs
    │
    ├── Views/                       # Razor UI templates
    ├── wwwroot/                     # Static assets (css/js/images)
    └── Program.cs                   # DI setup and middleware pipeline
```

## Architecture Layers

### 1. Domain and View Models (`Models/`)
- **Purpose**: Represent business entities and UI/API view data.
- **Key entities**:
  - `UserAccount`, `Product`, `Category`, `Order`, `OrderItem`
  - Enums for `UserRole` and `OrderStatus`
- **Responsibility**: Central business data shape and validation metadata.

### 2. Data Access Layer (`Data/`)
- **Purpose**: Persist and query application data through EF Core.
- **Implementation details**:
  - `ApplicationDbContext` defines DbSets and schema rules.
  - Decimal precision configured for price/amount fields.
  - Unique index on user email.
  - `SeedData` initializes baseline users/catalog data.
- **Responsibility**: Database communication and model mapping.

### 3. Web/API Controller Layer (`Controllers/`)
- **Purpose**: Handle incoming HTTP requests and orchestrate use cases.
- **Controller groups**:
  - Customer MVC controllers: catalog, cart, checkout, account.
  - Admin MVC controllers: dashboard, product management, order management.
  - API controllers (`Controllers/Api`): JWT login/register, product feed, mobile checkout/orders.
- **Responsibility**: Request validation, authorization boundaries, response shaping.

### 4. Infrastructure and Cross-Cutting Layer (`Program.cs`, `Extensions/`)
- **Purpose**: Wire authentication, authorization, session, and middleware.
- **Implementation details**:
  - Cookie authentication for MVC user sessions.
  - JWT Bearer authentication for API consumers (mobile app).
  - Session enabled for cart persistence.
  - Helper extensions for session object serialization and claim-based user access.
- **Responsibility**: Shared runtime concerns outside feature logic.

### 5. Presentation Layer (`Views/`, `wwwroot/`)
- **Purpose**: Render server-side UI for customer and admin workflows.
- **Responsibility**: Razor templates, forms, tables, validation messages, and client-side assets.

## Authentication and Authorization Strategy

- **MVC flow**: Cookie-based auth with role claims (`Admin`, `Customer`).
- **API flow**: JWT-based auth for mobile endpoints.
- **Policy usage**:
  - Admin controllers use `[Authorize(Roles = "Admin")]`.
  - Checkout/order history use `[Authorize]`.
  - API orders endpoints require JWT bearer scheme.

This dual-mode approach allows one backend to serve both web UI sessions and mobile API tokens.

## Data Flow

### Customer web checkout flow
```
User adds item -> CartController stores cart in Session (JSON)
               -> User opens /Orders/Checkout
               -> OrdersController builds order from cart + form data
               -> EF Core saves Order + OrderItems
               -> Session cart cleared
               -> Redirect to success page
```

### Mobile login and token flow
```
Mobile app POST /api/auth/login -> AuthApiController validates password hash
                               -> Generates JWT with user and role claims
                               -> Returns token payload
                               -> Mobile app sends Bearer token on next requests
```

### Mobile order placement flow
```
Mobile app POST /api/orders/checkout (JWT)
         -> OrdersApiController resolves user from claims
         -> Loads active products and validates request items
         -> Builds Order + OrderItem snapshot
         -> Saves via ApplicationDbContext
         -> Returns order summary
```

### Admin revenue dashboard flow
```
Admin opens dashboard with selected date
     -> AdminDashboardController filters paid orders
     -> Calculates day/month/year totals
     -> Builds 7-day label/value series
     -> Razor view renders KPI cards + chart data
```

## Key Benefits

1. **Single deployable unit**: MVC and API are hosted together for simpler operations.
2. **Shared domain model**: web and mobile features reuse the same entities and persistence.
3. **Role-secured modules**: clear boundary between customer and admin capabilities.
4. **Flexible auth modes**: cookie for browser UX, JWT for mobile integrations.
5. **Assignment-fit architecture**: practical, maintainable, and fast to demo.

## Building and Running

```bash
dotnet restore
dotnet build
dotnet run --project LaptopStoreWeb/PT_WEB.csproj
```

The application will:
1. Configure services and authentication schemes.
2. Connect to SQL Server via `DefaultConnection`.
3. Execute seed initialization on startup.
4. Expose MVC pages and API routes in one host.

## Future Enhancements

- Extract domain services from controllers to reduce controller responsibilities.
- Add repository/service abstractions for easier unit testing.
- Introduce FluentValidation for richer request validation.
- Add centralized exception-handling middleware for API error consistency.
- Add integration tests for checkout and admin dashboard calculations.