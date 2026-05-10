# PT_MOBILE_RN - Tech Stack Document

## 1. Project Overview

PT_MOBILE_RN is a cross-platform mobile app (Android/iOS/Web via Expo) for browsing and ordering laptops.
It integrates with the PT_WEB backend using REST APIs and JWT-based authentication.

```
PT_WEB (ASP.NET Core 8 + SQL Server)
      |
      | REST API (JSON over HTTP)
      | JWT Bearer Token
      v
PT_MOBILE_RN (React Native + Expo)
```

## 2. Core Technology Stack

### Runtime and Framework
- React 19.1.0
- React Native 0.81.5
- Expo SDK 54

### Navigation
- @react-navigation/native
- @react-navigation/native-stack
- @react-navigation/bottom-tabs
- react-native-screens
- react-native-safe-area-context
- react-native-gesture-handler

### Networking and API
- axios for HTTP client
- Central API client with request interceptor for JWT token injection

### Local Storage and Session
- @react-native-async-storage/async-storage
- Stores token and user profile locally

### UI and Theming
- React Native core components + StyleSheet
- @expo/vector-icons (Ionicons) for tab/navigation icons
- expo-status-bar
- Central color and formatting utilities in src/utils/constants.js

## 3. App Architecture

The app uses a lightweight layered structure:

- Presentation layer: screen components under src/screens
- State layer: React Context providers
  - AuthContext: authentication state (user, token, loading)
  - CartContext: cart state (items, quantities, totals)
- Data layer: API modules under src/api
  - client.js: axios instance + auth interceptor
  - index.js: endpoint-specific API wrappers (auth, products, orders)

Navigation structure:

- Root stack navigator
  - HomeTab (bottom tabs)
  - ProductDetail
  - Checkout
  - Auth (modal stack)
- Auth stack
  - Login
  - Register
- Bottom tabs
  - Home
  - Cart
  - Orders
  - Profile

## 4. Feature Coverage

### Product browsing
- Product list with search, category filter, and price sort
- Pagination via page query parameter and infinite scrolling
- Product details screen with quantity selector

### Cart and checkout
- In-memory cart management in CartContext
- Quantity update, remove item, clear cart
- Checkout payload maps cart items to { productId, quantity }

### Authentication and account
- Register/login via backend API
- JWT token persistence in AsyncStorage
- Conditional guest vs authenticated experiences

### Orders
- Fetch current user orders from protected endpoint
- Pull-to-refresh order list

## 5. Backend API Contract (Used by Mobile)

| Method | Endpoint | Auth | Purpose |
|--------|----------|------|---------|
| POST | /api/auth/register | No | Register new account |
| POST | /api/auth/login | No | Login and return JWT |
| GET | /api/products | No | Product listing with query params |
| GET | /api/products/{id} | No | Product detail |
| GET | /api/products/categories | No | Category list |
| POST | /api/orders/checkout | Yes | Place order |
| GET | /api/orders/my | Yes | Current user order history |

Supported product query params:

- search: string
- categoryId: number
- sort: price_asc | price_desc
- page: number

## 6. Configuration

### API base URL
Defined in src/api/client.js:

```js
const BASE_URL = 'http://10.0.2.2:5226';
```

Notes:
- 10.0.2.2 works for Android emulator to reach host localhost
- For physical devices, replace with host machine LAN IP

### Expo app config
Defined in app.json:

- orientation: portrait
- userInterfaceStyle: light
- newArchEnabled: true
- android.edgeToEdgeEnabled: true

## 7. Project Structure

```
PT_MOBILE_RN/
  App.js
  app.json
  package.json
  src/
    api/
      client.js
      index.js
    context/
      AuthContext.js
      CartContext.js
    screens/
      HomeScreen.js
      ProductDetailScreen.js
      CartScreen.js
      CheckoutScreen.js
      OrdersScreen.js
      ProfileScreen.js
      LoginScreen.js
      RegisterScreen.js
    utils/
      constants.js
```

## 8. Dependencies (package.json)

```json
{
  "dependencies": {
    "expo": "~54.0.33",
    "react": "19.1.0",
    "react-native": "0.81.5",
    "axios": "^1.15.2",
    "@react-native-async-storage/async-storage": "2.2.0",
    "@react-navigation/native": "^7.2.2",
    "@react-navigation/native-stack": "^7.14.12",
    "@react-navigation/bottom-tabs": "^7.15.11",
    "react-native-gesture-handler": "~2.28.0",
    "react-native-safe-area-context": "~5.6.0",
    "react-native-screens": "~4.16.0",
    "expo-status-bar": "~3.0.9",
    "expo-linear-gradient": "~15.0.8",
    "react-native-vector-icons": "^10.3.0"
  }
}
```

## 9. Build and Run Requirements

| Requirement | Version |
|-------------|---------|
| Node.js | 18+ recommended |
| npm | 9+ recommended |
| Expo CLI | via npx expo |
| Android Studio/Xcode | Required for native simulator/emulator |

Run commands:

```bash
npm install
npm run start
npm run android
npm run ios
npm run web
```

## 10. Current Technical Notes

- Cart data is session-scoped in memory (not persisted across app restarts)
- Auth state is persisted with AsyncStorage and restored on app launch
- Protected actions (checkout/orders) are gated by token presence in UI and API
- Error handling currently uses simple Alert messages (minimal but clear)

## 11. QA Checklist

- [ ] Start PT_WEB backend on port 5226
- [ ] Verify API base URL in src/api/client.js for current device
- [ ] Register and login successfully
- [ ] Browse products with search/filter/sort/pagination
- [ ] Add/update/remove cart items
- [ ] Place checkout as authenticated user
- [ ] View order history after checkout
