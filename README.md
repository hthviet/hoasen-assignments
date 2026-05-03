# NHEO - Laptop E-Commerce Platform

A full-stack e-commerce application for laptop sales featuring a .NET backend, web frontend, and React Native mobile app.

## Project Structure

```
NHEO/
├── APP/
│   ├── PT_WEB/               # ASP.NET Core backend & web frontend
│   │   └── LaptopStoreWeb/   # Main ASP.NET application
│   │       ├── Controllers/  # API and page controllers
│   │       ├── Models/       # Database models
│   │       ├── Views/        # Razor pages
│   │       ├── wwwroot/      # Static files (CSS, JS, images)
│   │       └── Data/         # Database context and migrations
│   └── PT_MOBILE_RN/         # React Native mobile app (Expo)
│       ├── src/
│       │   ├── screens/      # App screens (Home, Cart, Orders, etc.)
│       │   ├── components/   # Reusable React Native components
│       │   ├── context/      # Auth and Cart context providers
│       │   └── api/          # API client configuration
│       └── package.json
├── start-backend.sh          # Script to start ASP.NET backend
└── start-mobile-stack.sh     # Combined backend + Expo mobile startup

```

## Tech Stack

### Backend (PT_WEB)
- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server
- **Authentication**: JWT + Cookie-based auth
- **API**: RESTful endpoints for products, orders, auth

### Mobile (PT_MOBILE_RN)
- **Framework**: React Native with Expo
- **State Management**: React Context
- **Navigation**: React Navigation
- **HTTP Client**: Axios

## Features

- **User Authentication**: Register, login, logout
- **Product Catalog**: Browse laptops with pagination, search, and sorting
- **Shopping Cart**: Add/remove items, persistent storage
- **Checkout**: Order placement with shipping details
- **Order Management**: View order history and status
- **Admin Dashboard**: Product management, order tracking, revenue reports
- **Responsive Design**: Works on mobile and web

## Getting Started

### Prerequisites

- **.NET SDK** 8.0 or later
- **SQL Server** (local or remote)
- **Node.js** & **npm** (for React Native)
- **Android SDK** & **Emulator** (for mobile testing)
- **Expo CLI**: `npm install -g expo-cli`

### Backend Setup

1. Navigate to PT_WEB:
   ```bash
   cd APP/PT_WEB/LaptopStoreWeb
   ```

2. Configure database connection in `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=LaptopStoreDb;Trusted_Connection=true;"
     }
   }
   ```

3. Apply migrations:
   ```bash
   dotnet ef database update
   ```

4. Start backend:
   ```bash
   dotnet run
   ```
   Backend will run on `http://localhost:5226`

### Mobile App Setup

1. Navigate to PT_MOBILE_RN:
   ```bash
   cd APP/PT_MOBILE_RN
   npm install
   ```

2. Start Expo and run on Android emulator:
   ```bash
   npm run android
   ```

### One-Command Startup

To start both backend and mobile app simultaneously:

```bash
./start-mobile-stack.sh
```

This script will:
- Start ASP.NET backend on port 5226
- Launch Expo app on Metro bundler
- Handle port conflicts automatically

## API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login

### Products
- `GET /api/products` - List all products (paginated)
- `GET /api/products/{id}` - Get product details
- `GET /api/products/categories` - List categories

### Orders
- `POST /api/orders/checkout` - Place a new order
- `GET /api/orders/my` - Get user's orders

## Development

### Backend Development
- API runs in development mode with detailed logging
- HTTPS redirection is disabled in development
- Database seeding automatically runs on startup

### Mobile Development
- Metro bundler supports hot reload (press `r`)
- Debug menu: press `d` in Expo
- Logs: Check emulator logcat or Metro console

## Database

### Seed Data
The application automatically seeds initial data:
- 1 Admin user
- 2 Customer users
- 30+ Laptop products across multiple categories

### Schema
Key tables:
- `UserAccounts` - User profiles and credentials
- `Products` - Laptop inventory
- `Categories` - Product categories
- `Orders` - Customer orders
- `OrderItems` - Line items in orders

## Deployment

### Backend
- Publish to Azure App Service, AWS, or on-premises
- Configure SQL Server connection string for production
- Update JWT settings for production environment

### Mobile
- Build APK: `eas build --platform android --local`
- Build AAB for Google Play: `eas build --platform android --local --output=app-release.aab`

## Troubleshooting

### Backend won't start
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Review logs in console

### Mobile app can't connect to backend
- Ensure backend is running on port 5226
- On Android emulator: backend should be accessible at `http://10.0.2.2:5226`
- Check firewall settings

### Emulator won't launch
- Verify Android SDK is installed
- Run `emulator -avd Pixel_7_API_35` to start manually

## License

Proprietary - All rights reserved

## Contact

For questions or support, contact the development team.
