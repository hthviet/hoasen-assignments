# WinForms Clean Architecture

## Project Structure

The WinForms application has been refactored from a monolithic `Form1.cs` (723 lines) into a clean, layered architecture:

```
PT_WINFORM/
├── Models/              # Data Transfer Objects (DTOs)
│   ├── ProductDto.cs
│   ├── CategoryDto.cs
│   ├── OrderDto.cs
│   ├── LoginResponse.cs
│   └── ProductsResponse.cs
│
├── Services/            # API Client Layer
│   ├── AuthService.cs       # Authentication & JWT token management
│   ├── ProductService.cs    # Product catalog API calls
│   └── OrderService.cs      # Order & checkout API calls
│
├── Business/            # Business Logic Layer
│   └── CartManager.cs       # Shopping cart state management
│
├── UI/                  # Presentation Layer
│   ├── MainForm.cs          # Main application window
│   └── MainForm.Designer.cs # WinForms designer file
│
└── Program.cs           # Dependency injection & app startup
```

## Architecture Layers

### 1. **Models Layer** (`Models/`)
- **Purpose**: Define data contracts for API communication
- **Files**:
  - `ProductDto.cs`: Product information (id, name, brand, price, etc.)
  - `CategoryDto.cs`: Product category
  - `OrderDto.cs`: Order and order items
  - `LoginResponse.cs`: Authentication response with JWT token
  - `ProductsResponse.cs`: Paginated product list response
- **Responsibility**: Pure data structures, no business logic

### 2. **Services Layer** (`Services/`)
- **Purpose**: Handle API communication and business operations
- **Files**:
  - `AuthService.cs`: 
    - Login with email/password
    - JWT token management
    - Injects token into Authorization header
  - `ProductService.cs`:
    - Fetch product catalog with pagination
    - Search and filter by category
    - Fetch available categories
  - `OrderService.cs`:
    - Retrieve user's orders
    - Process checkout with items and shipping info
- **Responsibility**: Pure API communication, no UI concerns
- **Design Pattern**: Interfaces (`IAuthService`, `IProductService`, `IOrderService`) for testability

### 3. **Business Logic Layer** (`Business/`)
- **Purpose**: Manage domain-specific business operations
- **Files**:
  - `CartManager.cs`:
    - In-memory shopping cart management
    - Add/remove products
    - Calculate totals
    - Line-item management
- **Responsibility**: Business rules, independent of UI and API
- **Design Pattern**: Interface (`ICartManager`) for dependency injection

### 4. **UI Layer** (`UI/`)
- **Purpose**: User interface and event handling
- **Files**:
  - `MainForm.cs`:
    - Programmatic UI construction (sidebar, tabs, grids)
    - Event handlers for user interactions
    - Data binding to UI controls
    - Tab-based navigation (Dashboard, Products, Cart, Orders)
- **Responsibility**: Only UI rendering and event handling
- **Separation**: All business logic delegated to Services and Business layers

### 5. **Dependency Injection** (`Program.cs`)
- **Purpose**: Configure and wire all dependencies
- **Setup**:
  - `IAuthService` → `AuthService` (with HttpClient)
  - `IProductService` → `ProductService` (with HttpClient)
  - `IOrderService` → `OrderService` (with HttpClient)
  - `ICartManager` → `CartManager` (singleton)
- **Benefits**: Loose coupling, testability, easy to swap implementations

## Data Flow

### Product Browsing Flow:
```
User clicks "Refresh" → MainForm.LoadProductsAsync()
                    ↓
                ProductService.GetProductsAsync()
                    ↓
                HttpClient calls /api/products
                    ↓
                Returns ProductsResponse
                    ↓
                Binds to DataGridView
```

### Adding to Cart Flow:
```
User clicks "Add to Cart" → MainForm.AddSelectedProductToCart()
                        ↓
                CartManager.AddProduct()
                        ↓
                Updates internal List<CartLine>
                        ↓
                MainForm.RefreshCartGrid()
                        ↓
                Rebinds DataGridView
```

### Checkout Flow:
```
User clicks "Checkout" → MainForm.CheckoutAsync()
                     ↓
                OrderService.CheckoutAsync()
                     ↓
                HttpClient calls /api/orders/checkout
                     ↓
                CartManager.Clear()
                     ↓
                MainForm.LoadOrdersAsync()
                     ↓
                Displays order list
```

## Key Benefits

1. **Separation of Concerns**: Each layer has a single responsibility
2. **Testability**: Services and Business logic can be unit tested independently
3. **Maintainability**: Changes to UI don't affect business logic
4. **Reusability**: Services can be used by other UI frameworks (Console, ASP.NET, etc.)
5. **Scalability**: Easy to add new features or modify existing ones
6. **Dependency Injection**: Loose coupling through interfaces
7. **Reduced Complexity**: 723-line monolithic file split into focused, single-responsibility classes

## Building and Running

```bash
# Build the project
dotnet build

# Run the application
dotnet run
```

The application will:
1. Initialize dependency injection container
2. Instantiate all services with HttpClient pointing to `http://localhost:5226`
3. Launch MainForm with all dependencies injected
4. Automatically login as `customer1@gmail.com`
5. Load products, categories, and previous orders

## Future Enhancements

- Add unit tests for Services and Business logic
- Mock HttpClient for testing
- Add more UI forms (separate windows for product detail, order detail)
- Implement caching in ProductService
- Add retry logic for API calls
- Add user preferences and settings service
