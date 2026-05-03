# NHEO Laptop E-Commerce Platform - Use Cases

## Document Overview

This document describes the functional requirements and user interactions for the NHEO (Hoàn Sen) laptop e-commerce platform. The system supports customers browsing and purchasing laptops through web and mobile interfaces, as well as administrators managing products and orders.

## System Actors

### Primary Actors
1. **Customer (Guest/Registered User)**
   - Anonymous users who can browse products
   - Registered users who can purchase and track orders

2. **Administrator**
   - Manages product catalog and inventory
   - Manages orders and fulfillment
   - Views sales analytics and revenue reports

### Secondary Actors
1. **Payment System** - Processes transactions (future enhancement)
2. **Email Service** - Sends order confirmations and notifications (future enhancement)

---

## Use Cases

### 1. User Authentication & Account Management

#### UC-1.1: Register New Account
**Actors:** Customer (Guest)  
**Preconditions:** User not already registered  
**Main Flow:**
1. User navigates to registration page
2. System displays registration form (Full Name, Email, Password)
3. User enters information and submits
4. System validates email uniqueness
5. System creates user account and stores hashed password
6. System returns success message and redirects to login

**Alternative Flows:**
- Email already exists → System displays error message
- Invalid email format → System displays validation error
- Password too weak → System displays password requirements

**Postconditions:** New user account created, user can log in

---

#### UC-1.2: Login to Account
**Actors:** Customer (Guest/Registered User)  
**Preconditions:** User has registered account  
**Main Flow:**
1. User navigates to login page
2. System displays login form (Email, Password)
3. User enters credentials and submits
4. System validates credentials against database
5. System generates JWT token
6. System stores token in device storage
7. System redirects to home page with authenticated session

**Alternative Flows:**
- Invalid email/password → System displays error
- User account inactive → System displays account status message

**Postconditions:** User authenticated, JWT token stored locally

---

#### UC-1.3: View Profile
**Actors:** Customer (Registered User)  
**Preconditions:** User logged in  
**Main Flow:**
1. User navigates to Profile screen
2. System retrieves user data from database
3. System displays user information (Full Name, Email, Account Creation Date)

**Postconditions:** User profile displayed

---

#### UC-1.4: Logout from Account
**Actors:** Customer (Registered User)  
**Preconditions:** User logged in  
**Main Flow:**
1. User clicks Logout button on Profile screen
2. System clears stored JWT token
3. System redirects to login screen

**Postconditions:** User session terminated, token cleared

---

### 2. Product Catalog Management

#### UC-2.1: Browse Product Catalog
**Actors:** Customer (Guest/Registered)  
**Preconditions:** None  
**Main Flow:**
1. User navigates to Home/Products screen
2. System retrieves products from database with pagination (default 10 items per page)
3. System displays products with image, name, brand, price, stock status
4. User can scroll to view more products

**Alternative Flows:**
- No products available → System displays "No products available" message
- Database unavailable → System displays error message

**Postconditions:** Product catalog displayed

---

#### UC-2.2: Search Products
**Actors:** Customer (Guest/Registered)  
**Preconditions:** Product catalog loaded  
**Main Flow:**
1. User enters search term in search box
2. System sends search query to backend
3. System filters products by name/brand/description matching search term
4. System displays filtered results

**Alternative Flows:**
- No matches found → System displays "No results found"
- Search term empty → System displays all products

**Postconditions:** Filtered product list displayed

---

#### UC-2.3: Filter & Sort Products
**Actors:** Customer (Guest/Registered)  
**Preconditions:** Product catalog loaded  
**Main Flow:**
1. User selects filter criteria (Category, Price Range)
2. User selects sort option (Price: Low→High, Name: A→Z)
3. System applies filters and sorting to product list
4. System displays updated results

**Alternative Flows:**
- No products match filters → System displays empty state
- Multiple filters applied → System applies all filters (AND logic)

**Postconditions:** Filtered and sorted product list displayed

---

#### UC-2.4: View Product Details
**Actors:** Customer (Guest/Registered)  
**Preconditions:** Product catalog displayed  
**Main Flow:**
1. User taps/clicks on product item
2. System retrieves full product details from database
3. System displays:
   - Product image gallery
   - Name, brand, price
   - Category
   - Full description
   - Stock quantity
   - "Add to Cart" button
4. User can add product to cart or return to catalog

**Postconditions:** Product details displayed

---

### 3. Shopping Cart Management

#### UC-3.1: Add Product to Cart
**Actors:** Customer (Guest/Registered)  
**Preconditions:** Viewing product details  
**Main Flow:**
1. User specifies quantity (default 1)
2. User clicks "Add to Cart" button
3. System validates product availability and stock
4. System stores product in local cart (AsyncStorage)
5. System displays "Added to cart" confirmation
6. System updates cart item count in tab badge

**Alternative Flows:**
- Insufficient stock → System displays "Out of stock" message
- Product not available → System displays error

**Postconditions:** Product added to cart, cart count updated

---

#### UC-3.2: View Shopping Cart
**Actors:** Customer (Guest/Registered)  
**Preconditions:** At least one item in cart  
**Main Flow:**
1. User navigates to Cart screen
2. System retrieves cart items from local storage
3. System displays:
   - List of cart items (name, quantity, unit price, line total)
   - Total cart value
   - "Checkout" and "Continue Shopping" buttons
4. User can remove items or proceed to checkout

**Alternative Flows:**
- Cart is empty → System displays "Cart is empty" message

**Postconditions:** Cart contents displayed

---

#### UC-3.3: Modify Cart Item Quantity
**Actors:** Customer (Guest/Registered)  
**Preconditions:** Cart displayed  
**Main Flow:**
1. User adjusts quantity using +/- buttons or input field
2. System validates new quantity against stock
3. System updates cart total
4. System persists changes to local storage

**Alternative Flows:**
- New quantity exceeds stock → System displays "Insufficient stock"
- Quantity set to 0 → System removes item from cart

**Postconditions:** Cart item quantity updated, total recalculated

---

#### UC-3.4: Remove Product from Cart
**Actors:** Customer (Guest/Registered)  
**Preconditions:** Cart displayed  
**Main Flow:**
1. User clicks remove button on cart item
2. System confirms removal
3. System removes item from cart
4. System persists changes
5. System updates cart count badge

**Postconditions:** Item removed from cart

---

### 4. Checkout & Order Placement

#### UC-4.1: Initiate Checkout
**Actors:** Customer (Registered User)  
**Preconditions:** 
- User logged in
- Cart contains items
**Main Flow:**
1. User clicks "Checkout" button from cart
2. System verifies user authentication
3. System displays checkout form with fields:
   - Shipping Address
   - Phone Number
   - Order Summary (items, quantities, total)
4. User enters shipping details
5. User reviews order summary

**Alternative Flows:**
- User not logged in → System redirects to login screen
- Cart is empty → System displays warning

**Postconditions:** Checkout screen displayed with order details

---

#### UC-4.2: Place Order
**Actors:** Customer (Registered User)  
**Preconditions:** Checkout screen displayed with filled details  
**Main Flow:**
1. User clicks "Place Order" button
2. System validates form fields (phone, address not empty)
3. System sends checkout request to backend:
   - User ID (from JWT token)
   - Shipping address
   - Phone number
   - Cart items (product IDs, quantities)
4. Backend validates stock availability
5. Backend creates Order record in database
6. Backend creates OrderItems for each cart product
7. Backend returns Order ID to frontend
8. System clears local cart storage
9. System displays success message with Order ID
10. User can navigate to Orders screen to track order

**Alternative Flows:**
- Stock unavailable for item → System displays "Item no longer available"
- Server error → System displays "Checkout failed, please try again"
- Network error → System displays error message

**Postconditions:** Order created, cart cleared, order ID provided to user

---

### 5. Order Management

#### UC-5.1: View Order History
**Actors:** Customer (Registered User)  
**Preconditions:** User logged in  
**Main Flow:**
1. User navigates to Orders screen
2. System retrieves user's orders from backend (authenticated via JWT)
3. System displays list of orders with:
   - Order ID
   - Order date and time
   - Order status (Pending/Processing/Shipped/Delivered/Cancelled)
   - Shipping address
   - Phone number
   - Items summary
   - Total amount
4. User can refresh or tap individual orders

**Alternative Flows:**
- User has no orders → System displays "No orders yet" message
- Network error → System displays error message

**Postconditions:** Order history displayed

---

#### UC-5.2: View Order Details
**Actors:** Customer (Registered User)  
**Preconditions:** Order history displayed  
**Main Flow:**
1. User taps on order from history
2. System retrieves detailed order information
3. System displays:
   - Order ID
   - Order date
   - Status timeline
   - Shipping address and phone
   - Line items (product name, quantity, unit price, line total)
   - Order total
4. User can return to order history

**Postconditions:** Order details displayed

---

### 6. Admin Dashboard & Management

#### UC-6.1: Access Admin Dashboard
**Actors:** Administrator  
**Preconditions:** 
- User is admin (role-based check)
- User logged in
**Main Flow:**
1. Admin navigates to admin dashboard URL
2. System verifies admin role from JWT token
3. System displays dashboard with:
   - Revenue overview
   - Order statistics
   - Product count
   - Navigation links to management sections

**Alternative Flows:**
- User not logged in → System redirects to login
- User is not admin → System displays "Access Denied"

**Postconditions:** Admin dashboard displayed

---

#### UC-6.2: Manage Products (Create/Read/Update)
**Actors:** Administrator  
**Preconditions:** Admin logged in  
**Main Flow (Create):**
1. Admin clicks "Add Product" button
2. System displays product form with fields:
   - Product name, brand, description
   - Category
   - Price, stock quantity
   - Image URL
3. Admin fills form and submits
4. System validates required fields
5. System creates product in database
6. System displays success message

**Main Flow (Update):**
1. Admin selects product from list
2. System displays product form with current data
3. Admin modifies fields and submits
4. System validates and updates product
5. System displays success message

**Main Flow (Delete):**
1. Admin selects product and confirms deletion
2. System soft-deletes product (marks IsActive=false)
3. System removes from product catalog

**Postconditions:** Product created/updated/deleted

---

#### UC-6.3: Track Orders
**Actors:** Administrator  
**Preconditions:** Admin logged in  
**Main Flow:**
1. Admin navigates to Orders management
2. System displays all orders (not just user's)
3. Admin can filter by:
   - Status (Pending, Processing, Shipped, Delivered, Cancelled)
   - Date range
   - Customer
4. System displays orders with status, customer, total, date
5. Admin can click order to view details and update status

**Postconditions:** Order list displayed with filter options

---

#### UC-6.4: Update Order Status
**Actors:** Administrator  
**Preconditions:** Order details displayed  
**Main Flow:**
1. Admin clicks "Update Status" button
2. System displays status dropdown (Pending → Processing → Shipped → Delivered)
3. Admin selects new status and submits
4. System updates order status in database
5. System displays success message

**Postconditions:** Order status updated

---

#### UC-6.5: View Revenue Reports
**Actors:** Administrator  
**Preconditions:** Admin logged in  
**Main Flow:**
1. Admin navigates to Reports section
2. System displays revenue analytics:
   - Total revenue
   - Orders by date/week/month
   - Top selling products
   - Revenue trends chart
3. Admin can filter by date range

**Postconditions:** Revenue reports displayed

---

## Non-Functional Requirements

### Performance
- Product catalog loads in < 2 seconds
- Search results return in < 1 second
- Checkout completes in < 3 seconds

### Security
- Password stored as hashed (BCrypt)
- JWT tokens expire after 24 hours
- HTTPS used for all communications
- SQL injection prevention via parameterized queries
- CORS enabled only for frontend domains

### Availability
- System available 99.5% uptime
- Graceful error handling for backend failures

### Usability
- Mobile-responsive design
- Intuitive navigation
- Clear confirmation messages
- Support for both English and Vietnamese (future)

---

## Data Entities

### User Account
- ID, Full Name, Email, Password (hashed), Role, CreatedDate, UpdatedDate

### Product
- ID, Name, Brand, Description, Price, Category, StockQuantity, ImageUrl, IsActive

### Category
- ID, Name

### Order
- ID, UserID, OrderDate, Status, ShippingAddress, PhoneNumber, TotalAmount

### OrderItem
- ID, OrderID, ProductID, Quantity, UnitPrice, LineTotal

---

## System Architecture

```
┌─────────────────────────────────────────────┐
│ Frontend Layer                               │
├─────────────────────────────────────────────┤
│ - Web (ASP.NET Razor Views)                 │
│ - Mobile (React Native Expo / Native Android)
└────────────────┬────────────────────────────┘
                 │
        ┌────────▼─────────┐
        │ API Layer        │
        │ (ASP.NET Core)   │
        │ JWT Auth, REST   │
        └────────┬─────────┘
                 │
        ┌────────▼──────────────┐
        │ Business Logic Layer  │
        ├──────────────────────┤
        │ - Auth Service       │
        │ - Product Service    │
        │ - Order Service      │
        │ - Admin Service      │
        └────────┬──────────────┘
                 │
        ┌────────▼─────────────┐
        │ Data Access Layer    │
        │ (Entity Framework)   │
        └────────┬─────────────┘
                 │
        ┌────────▼─────────────┐
        │ SQL Server Database  │
        └──────────────────────┘
```

---

## Future Use Cases (Out of Scope - Phase 2)

1. **Payment Integration** - Online payment gateway (Stripe, PayPal)
2. **Email Notifications** - Order confirmation and status update emails
3. **Wishlist/Favorites** - Save products for later
4. **Product Reviews & Ratings** - Customer feedback
5. **Discount/Coupon Codes** - Promotional offers
6. **Inventory Alerts** - Low stock notifications
7. **Analytics Dashboard** - Detailed business metrics
8. **Multi-language Support** - Vietnamese/English localization
9. **Push Notifications** - Mobile app notifications
10. **Order Tracking** - Real-time delivery tracking

---

## Document Version

- **Version:** 1.0
- **Date:** May 3, 2026
- **Author:** Development Team
- **Status:** Active
