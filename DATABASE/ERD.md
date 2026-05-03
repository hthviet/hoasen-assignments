# NHEO Database ERD

## Overview

This ERD is based on the EF Core models in APP/PT_WEB/LaptopStoreWeb.

## Entity Relationship Diagram

```mermaid
erDiagram
    USER_ACCOUNTS {
        int Id PK
        string FullName
        string Email UK
        string PasswordHash
        int Role
        datetime CreatedAt
    }

    CATEGORIES {
        int Id PK
        string Name
        string Description
    }

    PRODUCTS {
        int Id PK
        string Name
        string Brand
        string Description
        decimal Price
        string ImageUrl
        bool IsActive
        datetime CreatedAt
        int CategoryId FK
    }

    ORDERS {
        int Id PK
        int UserAccountId FK
        datetime OrderDate
        decimal TotalAmount
        string Status
        string ShippingAddress
        string PhoneNumber
    }

    ORDER_ITEMS {
        int Id PK
        int OrderId FK
        int ProductId
        string ProductName
        decimal UnitPrice
        int Quantity
        decimal LineTotal
    }

    USER_ACCOUNTS ||--o{ ORDERS : places
    CATEGORIES ||--o{ PRODUCTS : contains
    ORDERS ||--|{ ORDER_ITEMS : includes
```

## Notes

- Table names shown in uppercase map to EF entities:
  - USER_ACCOUNTS -> UserAccount
  - CATEGORIES -> Category
  - PRODUCTS -> Product
  - ORDERS -> Order
  - ORDER_ITEMS -> OrderItem
- Decimal precision configured in ApplicationDbContext:
  - Product.Price: decimal(18,2)
  - Order.TotalAmount: decimal(18,2)
  - OrderItem.UnitPrice: decimal(18,2)
  - OrderItem.LineTotal: decimal(18,2)
- Unique index:
  - UserAccount.Email is unique
- Enum/string domains:
  - UserAccount.Role: Customer (0), Admin (1)
  - Order.Status: New, Shipped, Paid
- OrderItem.ProductId is stored as a product reference field in the model.
