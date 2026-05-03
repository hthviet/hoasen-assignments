# PT_MOBILE – Tech Stack Document

## 1. Project Overview

PT_MOBILE is a native Android application that allows customers to browse and purchase laptops from the Laptop Store.
It connects to the existing PT_WEB backend through a REST API with JWT authentication.

```
PT_WEB (ASP.NET Core 8 + SQL Server)
      │
      │  REST API (JSON over HTTPS)
      │  JWT Bearer Token
      ▼
PT_MOBILE (Android – Java – Android Studio)
```

---

## 2. Backend – PT_WEB API Layer

### Framework
- ASP.NET Core 8 MVC + Web API
- Language: C#

### Authentication
- Cookie auth for the web interface (unchanged)
- JWT Bearer token for the mobile API

### JWT Configuration (appsettings.json)
```json
"Jwt": {
  "Key": "LaptopStoreSuperSecretKey2026XyZ!@#$",
  "Issuer": "LaptopStoreApi",
  "Audience": "LaptopStoreMobile"
}
```

### API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | /api/auth/register | None | Register new customer |
| POST | /api/auth/login | None | Login, returns JWT token |
| GET | /api/products | None | List products (search, filter, sort, paginate) |
| GET | /api/products/{id} | None | Get product detail |
| GET | /api/products/categories | None | List all categories |
| POST | /api/orders/checkout | JWT | Place an order |
| GET | /api/orders/my | JWT | Get my order history |

### Query parameters for GET /api/products
| Param | Type | Description |
|-------|------|-------------|
| search | string | Filter by product name |
| categoryId | int | Filter by category |
| sort | string | `price_asc` or `price_desc` |
| page | int | Page number (default 1) |

### Database
- SQL Server
- ORM: Entity Framework Core 8

---

## 3. Mobile – PT_MOBILE

### Language
- Java

### IDE
- Android Studio (Ladybug or newer)

### Minimum SDK
- API level 24 (Android 7.0)

### Target SDK
- API level 34 (Android 14)

### Architecture
- Single Activity with Fragment navigation
- Simple MVC/direct pattern (student level)

### Key Libraries

| Library | Purpose |
|---------|---------|
| Retrofit 2 | HTTP client for API calls |
| OkHttp 3 | Underlying HTTP engine + logging interceptor |
| Gson | JSON serialization/deserialization |
| Glide | Image loading from URL |
| RecyclerView | Product list, cart list, order list |
| ViewPager2 + BottomNavigationView | Main navigation |
| SharedPreferences | Store JWT token locally |
| Material Components | UI styling |
| ConstraintLayout | Layout |

---

## 4. Screen Map

```
SplashActivity
    │
    ├── MainActivity (BottomNav: Home / Search / Cart / Orders / Account)
    │       ├── HomeFragment        – Featured products
    │       ├── ProductListFragment – Browse, search, filter, sort, paginate
    │       ├── ProductDetailActivity – Detail + Add to cart
    │       ├── CartFragment        – Cart items, update qty, delete, checkout
    │       ├── OrdersFragment      – Order history (requires login)
    │       └── AccountFragment     – Login / Register / Logout
    │
    ├── LoginActivity
    └── RegisterActivity
```

---

## 5. Data Flow

### Guest flow
1. App starts → fetch products from `/api/products`
2. User browses, adds item to local cart (in-memory list)
3. User taps Checkout → redirect to login if not authenticated

### Authenticated flow
1. User logs in → JWT token saved in SharedPreferences
2. Token attached as `Authorization: Bearer <token>` header on every protected call
3. Checkout → POST `/api/orders/checkout` with cart items + address + phone
4. Success → confirmation screen + clear cart
5. View order history → GET `/api/orders/my`

---

## 6. Project Structure (Android)

```
PT_MOBILE/
  app/
    src/main/
      java/com/laptopstore/ptmobile/
        activities/
          MainActivity.java
          LoginActivity.java
          RegisterActivity.java
          ProductDetailActivity.java
        fragments/
          HomeFragment.java
          ProductListFragment.java
          CartFragment.java
          OrdersFragment.java
          AccountFragment.java
        adapters/
          ProductAdapter.java
          CartAdapter.java
          OrderAdapter.java
        models/
          Product.java
          Category.java
          CartItem.java
          Order.java
          OrderItem.java
          LoginRequest.java
          RegisterRequest.java
          CheckoutRequest.java
          AuthResponse.java
        network/
          ApiClient.java
          ApiService.java
          AuthInterceptor.java
        utils/
          SessionManager.java
          CartManager.java
      res/
        layout/
        drawable/
        values/
    build.gradle
  build.gradle
  settings.gradle
```

---

## 7. Cart Strategy

Cart is kept in-memory via a singleton `CartManager` class during the session.
No cart persistence to server – on checkout the full cart is submitted in one POST.
This keeps the approach simple and matches the assignment scope.

---

## 8. API Base URL Configuration

In `ApiClient.java`, set:
```java
private static final String BASE_URL = "http://10.0.2.2:5226/"; // emulator
// private static final String BASE_URL = "http://<your-local-ip>:5226/"; // physical device
```

`10.0.2.2` is the Android Emulator alias for the host machine's `localhost`.

---

## 9. Dependencies (app/build.gradle)

```gradle
dependencies {
    implementation 'com.squareup.retrofit2:retrofit:2.9.0'
    implementation 'com.squareup.retrofit2:converter-gson:2.9.0'
    implementation 'com.squareup.okhttp3:logging-interceptor:4.12.0'
    implementation 'com.github.bumptech.glide:glide:4.16.0'
    implementation 'androidx.recyclerview:recyclerview:1.3.2'
    implementation 'androidx.viewpager2:viewpager2:1.1.0'
    implementation 'com.google.android.material:material:1.12.0'
    implementation 'androidx.constraintlayout:constraintlayout:2.1.4'
}
```

---

## 10. Build Requirements

| Requirement | Version |
|-------------|---------|
| Android Studio | Ladybug 2024.2+ |
| JDK | 17 |
| Gradle | 8.x |
| Compile SDK | 34 |
| Min SDK | 24 |

---

## 11. Delivery Checklist

- [ ] Backend API running on `http://localhost:5226`
- [ ] Android emulator or physical device connected
- [ ] `BASE_URL` set correctly in `ApiClient.java`
- [ ] Build and run in Android Studio
- [ ] Test: Register → Browse → Add to cart → Checkout → View orders
