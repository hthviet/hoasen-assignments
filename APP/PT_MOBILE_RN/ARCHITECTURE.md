# React Native Architecture

## Project Structure

The mobile application uses a layered, feature-oriented React Native architecture with Expo and React Navigation:

```
PT_MOBILE_RN/
├── src/
│   ├── api/                 # Data access layer
│   │   ├── client.js            # Axios client + JWT request interceptor
│   │   └── index.js             # Endpoint wrappers (auth, products, orders)
│   │
│   ├── context/             # Application state layer
│   │   ├── AuthContext.js       # Auth/session state persisted in AsyncStorage
│   │   └── CartContext.js       # In-memory cart and cart computations
│   │
│   ├── screens/             # Presentation layer
│   │   ├── HomeScreen.js
│   │   ├── ProductDetailScreen.js
│   │   ├── CartScreen.js
│   │   ├── CheckoutScreen.js
│   │   ├── OrdersScreen.js
│   │   ├── ProfileScreen.js
│   │   ├── LoginScreen.js
│   │   └── RegisterScreen.js
│   │
│   └── utils/
│       └── constants.js         # Shared UI constants (colors, spacing, etc.)
│
├── App.js                    # Root providers + navigation composition
├── app.json                  # Expo runtime configuration
└── package.json              # Dependencies and scripts
```

## Architecture Layers

### 1. Models and Contracts (`src/api/index.js`)
- **Purpose**: Define request/response contract boundaries through API wrapper methods.
- **Modules**:
  - `authApi`: login/register operations
  - `productsApi`: product list/detail/categories
  - `ordersApi`: checkout and current user orders
- **Responsibility**: Keep endpoint paths and payload contracts centralized.

### 2. Data Access Layer (`src/api/client.js`, `src/api/index.js`)
- **Purpose**: Isolate HTTP communication from screens.
- **Implementation details**:
  - Axios instance configured with shared base URL and timeout.
  - Request interceptor reads `token` from AsyncStorage and injects `Authorization: Bearer <token>`.
- **Responsibility**: Network concerns only (transport, auth header, endpoint invocation).

### 3. State Layer (`src/context/`)
- **Purpose**: Manage cross-screen application state.
- **Modules**:
  - `AuthContext.js`:
    - Restores `token` and `user` from AsyncStorage on startup.
    - Exposes `signIn` and `signOut` actions.
    - Tracks bootstrapping state with `loading`.
  - `CartContext.js`:
    - Manages cart lines in memory.
    - Supports add/remove/update quantity/clear operations.
    - Computes `totalItems` and `totalPrice`.
- **Responsibility**: Stateful business behavior independent from navigation and UI rendering.

### 4. Presentation Layer (`src/screens/`)
- **Purpose**: Render UI and orchestrate user interactions.
- **Responsibility**:
  - Fetch data through API layer.
  - Read/write shared state via contexts.
  - Trigger navigation transitions.
  - Display loading, empty, and error states.

### 5. Navigation and Composition (`App.js`)
- **Purpose**: Compose app-level structure and route hierarchy.
- **Navigation graph**:
  - Root stack:
    - `HomeTab` (bottom tabs)
    - `ProductDetail`
    - `Checkout`
    - `Auth` (modal stack)
  - Auth stack:
    - `Login`
    - `Register`
  - Bottom tabs:
    - `Home`, `Cart`, `Orders`, `Profile`
- **Provider composition**:
  - `AuthProvider` -> `CartProvider` -> `NavigationContainer`

## Data Flow

### App bootstrap and session restore
```
App mounts -> AuthProvider useEffect()
          -> AsyncStorage reads token/user
          -> Restore auth state or anonymous state
          -> AppNavigator renders routes
```

### Product browsing flow
```
User opens Home -> HomeScreen requests productsApi.getAll(params)
               -> api client calls GET /api/products
               -> Response mapped to screen list state
               -> User navigates to ProductDetail for item details
```

### Login flow
```
User submits login form -> authApi.login(email, password)
                       -> POST /api/auth/login
                       -> Receives JWT + user payload
                       -> AuthContext.signIn() persists AsyncStorage
                       -> Protected features become available
```

### Checkout flow
```
User taps checkout -> CheckoutScreen builds items payload from CartContext
                  -> ordersApi.checkout(address, phone, items)
                  -> POST /api/orders/checkout with Bearer token
                  -> Backend creates order
                  -> CartContext.clearCart()
                  -> OrdersScreen refreshes GET /api/orders/my
```

## Key Benefits

1. **Clear separation of concerns**: networking, state, and UI are split cleanly.
2. **Reusable state primitives**: auth/cart logic shared across multiple screens.
3. **Scalable navigation**: stack + tab composition supports feature growth.
4. **Consistent API usage**: all endpoints routed through one client and wrapper layer.
5. **Simple onboarding**: project structure maps directly to runtime responsibilities.

## Building and Running

```bash
npm install
npm run start
npm run android
npm run ios
npm run web
```

The application will:
1. Start Expo runtime.
2. Initialize providers and navigation.
3. Restore auth session from AsyncStorage.
4. Connect to PT_WEB backend (default base URL in `src/api/client.js`).

## Future Enhancements

- Persist cart state to AsyncStorage for app-restart continuity.
- Add API error normalization and retry/backoff strategy.
- Introduce screen-level hooks (for example `useProducts`, `useOrders`) to reduce duplicated request logic.
- Add unit tests for `AuthContext` and `CartContext` actions.
- Add environment-based API URL configuration for emulator, device, and production.