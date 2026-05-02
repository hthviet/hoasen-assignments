/*
    LaptopStoreDb_seed.sql
    Purpose: Create LaptopStoreDb schema and seed starter data.
    Note: This script will DROP and recreate LaptopStoreDb.
*/

USE master;
GO

IF DB_ID('LaptopStoreDb') IS NOT NULL
BEGIN
    ALTER DATABASE LaptopStoreDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LaptopStoreDb;
END
GO

CREATE DATABASE LaptopStoreDb;
GO

USE LaptopStoreDb;
GO

CREATE TABLE UserAccounts (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);
GO

CREATE UNIQUE INDEX IX_UserAccounts_Email ON UserAccounts(Email);
GO

CREATE TABLE Categories (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(250) NULL
);
GO

CREATE TABLE Products (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Brand NVARCHAR(80) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL,
    ImageUrl NVARCHAR(MAX) NOT NULL,
    IsActive BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    CategoryId INT NOT NULL,
    CONSTRAINT FK_Products_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
GO

CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
GO

CREATE TABLE Orders (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserAccountId INT NOT NULL,
    OrderDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    ShippingAddress NVARCHAR(250) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_Orders_UserAccounts_UserAccountId FOREIGN KEY (UserAccountId) REFERENCES UserAccounts(Id)
);
GO

CREATE INDEX IX_Orders_UserAccountId ON Orders(UserAccountId);
GO

CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(MAX) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);
GO

CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
GO

INSERT INTO UserAccounts (FullName, Email, PasswordHash, Role, CreatedAt)
VALUES
    (N'Admin User', N'admin@laptopstore.com', N'AQAAAAIAAYagAAAAEH8Ikj6WHp3RxhIulhuxfPXNqIYY+xXAsLxLbR0uxSOw6UXltsR8YiXqmYG25wKZFw==', 1, SYSUTCDATETIME()),
    (N'Nguyen Van A', N'customer1@gmail.com', N'AQAAAAIAAYagAAAAEAc8Yd7pOOTetFDeSPG1C8c2Kbf5DDNsQSg+3yIYPemySZq2f4MKqo6KERFf3YSg2Q==', 0, SYSUTCDATETIME()),
    (N'Tran Thi B', N'customer2@gmail.com', N'AQAAAAIAAYagAAAAEKBFbM3+fA5mxjl2OrwUR6YMjlGHM+2atJRcZl4vJcny3MMqQlMDtY0ySVO9niW39w==', 0, SYSUTCDATETIME());
GO

INSERT INTO Categories (Name, Description)
VALUES
    (N'Gaming', N'Gaming laptops'),
    (N'Office', N'Office laptops'),
    (N'Ultrabook', N'Thin and light laptops'),
    (N'Student', N'Affordable laptops for students');
GO

DECLARE @GamingId INT = (SELECT Id FROM Categories WHERE Name = N'Gaming');
DECLARE @OfficeId INT = (SELECT Id FROM Categories WHERE Name = N'Office');
DECLARE @UltrabookId INT = (SELECT Id FROM Categories WHERE Name = N'Ultrabook');
DECLARE @StudentId INT = (SELECT Id FROM Categories WHERE Name = N'Student');

INSERT INTO Products (Name, Brand, Description, Price, StockQuantity, ImageUrl, IsActive, CreatedAt, CategoryId)
VALUES
    (N'Acer Nitro 5', N'Acer', N'Acer Nitro 5 is a good laptop for study, work, and daily use.', 12000000, 5, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @GamingId),
    (N'Asus TUF F15', N'Asus', N'Asus TUF F15 is a good laptop for study, work, and daily use.', 12850000, 6, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @OfficeId),
    (N'Dell G15', N'Dell', N'Dell G15 is a good laptop for study, work, and daily use.', 13700000, 7, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @UltrabookId),
    (N'HP Victus 16', N'HP', N'HP Victus 16 is a good laptop for study, work, and daily use.', 14550000, 8, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @StudentId),
    (N'Lenovo Legion 5', N'Lenovo', N'Lenovo Legion 5 is a good laptop for study, work, and daily use.', 15400000, 9, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @GamingId),
    (N'MSI Katana', N'MSI', N'MSI Katana is a good laptop for study, work, and daily use.', 16250000, 10, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @OfficeId),
    (N'MacBook Air M2', N'MacBook', N'MacBook Air M2 is a good laptop for study, work, and daily use.', 17100000, 11, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @UltrabookId),
    (N'Dell Inspiron 15', N'Dell', N'Dell Inspiron 15 is a good laptop for study, work, and daily use.', 17950000, 12, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @StudentId),
    (N'HP Pavilion 14', N'HP', N'HP Pavilion 14 is a good laptop for study, work, and daily use.', 18800000, 13, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @GamingId),
    (N'Asus VivoBook 15', N'Asus', N'Asus VivoBook 15 is a good laptop for study, work, and daily use.', 19650000, 14, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @OfficeId),
    (N'Lenovo IdeaPad Slim', N'Lenovo', N'Lenovo IdeaPad Slim is a good laptop for study, work, and daily use.', 20500000, 15, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @UltrabookId),
    (N'Acer Aspire 7', N'Acer', N'Acer Aspire 7 is a good laptop for study, work, and daily use.', 21350000, 16, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @StudentId),
    (N'Gigabyte G5', N'Gigabyte', N'Gigabyte G5 is a good laptop for study, work, and daily use.', 22200000, 17, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @GamingId),
    (N'MSI Modern 14', N'MSI', N'MSI Modern 14 is a good laptop for study, work, and daily use.', 23050000, 18, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @OfficeId),
    (N'Dell XPS 13', N'Dell', N'Dell XPS 13 is a good laptop for study, work, and daily use.', 23900000, 19, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @UltrabookId),
    (N'HP Envy 13', N'HP', N'HP Envy 13 is a good laptop for study, work, and daily use.', 24750000, 20, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @StudentId),
    (N'Asus Zenbook 14', N'Asus', N'Asus Zenbook 14 is a good laptop for study, work, and daily use.', 25600000, 21, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @GamingId),
    (N'Lenovo Yoga Slim 7', N'Lenovo', N'Lenovo Yoga Slim 7 is a good laptop for study, work, and daily use.', 26450000, 22, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @OfficeId),
    (N'Acer Swift 3', N'Acer', N'Acer Swift 3 is a good laptop for study, work, and daily use.', 27300000, 23, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @UltrabookId),
    (N'Huawei MateBook D14', N'Huawei', N'Huawei MateBook D14 is a good laptop for study, work, and daily use.', 28150000, 24, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @StudentId),
    (N'LG Gram 16', N'LG', N'LG Gram 16 is a good laptop for study, work, and daily use.', 29000000, 25, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @GamingId),
    (N'Razer Blade 15', N'Razer', N'Razer Blade 15 is a good laptop for study, work, and daily use.', 29850000, 26, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @OfficeId),
    (N'Asus ROG Strix G16', N'Asus', N'Asus ROG Strix G16 is a good laptop for study, work, and daily use.', 30700000, 27, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @UltrabookId),
    (N'Lenovo LOQ 15', N'Lenovo', N'Lenovo LOQ 15 is a good laptop for study, work, and daily use.', 31550000, 28, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @StudentId),
    (N'MSI Thin GF63', N'MSI', N'MSI Thin GF63 is a good laptop for study, work, and daily use.', 32400000, 29, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @GamingId),
    (N'Dell Latitude 5440', N'Dell', N'Dell Latitude 5440 is a good laptop for study, work, and daily use.', 33250000, 30, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @OfficeId),
    (N'HP ProBook 450', N'HP', N'HP ProBook 450 is a good laptop for study, work, and daily use.', 34100000, 31, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @UltrabookId),
    (N'Acer TravelMate', N'Acer', N'Acer TravelMate is a good laptop for study, work, and daily use.', 34950000, 32, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @StudentId),
    (N'Asus ExpertBook', N'Asus', N'Asus ExpertBook is a good laptop for study, work, and daily use.', 35800000, 33, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @GamingId),
    (N'Lenovo ThinkBook 14', N'Lenovo', N'Lenovo ThinkBook 14 is a good laptop for study, work, and daily use.', 36650000, 34, N'/images/products/placeholder.svg', 1, SYSUTCDATETIME(), @OfficeId);
GO

;WITH NumberedProducts AS (
    SELECT Id, Brand
    FROM Products
)
UPDATE ProductData
SET ImageUrl = CASE NumberedProducts.Brand
    WHEN N'Acer' THEN N'https://images.pexels.com/photos/18105/pexels-photo.jpg'
    WHEN N'Asus' THEN N'https://images.pexels.com/photos/1229861/pexels-photo-1229861.jpeg'
    WHEN N'Dell' THEN N'https://images.pexels.com/photos/205421/pexels-photo-205421.jpeg'
    WHEN N'HP' THEN N'https://images.pexels.com/photos/7974/pexels-photo.jpg'
    WHEN N'Lenovo' THEN N'https://images.pexels.com/photos/374074/pexels-photo-374074.jpeg'
    WHEN N'MSI' THEN N'https://images.pexels.com/photos/1181671/pexels-photo-1181671.jpeg'
    WHEN N'MacBook' THEN N'https://images.pexels.com/photos/1779487/pexels-photo-1779487.jpeg'
    WHEN N'Gigabyte' THEN N'https://images.pexels.com/photos/303383/pexels-photo-303383.jpeg'
    WHEN N'Huawei' THEN N'https://images.pexels.com/photos/2148216/pexels-photo-2148216.jpeg'
    WHEN N'LG' THEN N'https://images.pexels.com/photos/267350/pexels-photo-267350.jpeg'
    WHEN N'Razer' THEN N'https://images.pexels.com/photos/109371/pexels-photo-109371.jpeg'
    ELSE N'https://images.pexels.com/photos/250459/pexels-photo-250459.jpeg'
END
FROM Products AS ProductData
INNER JOIN NumberedProducts ON NumberedProducts.Id = ProductData.Id;
GO

DECLARE @CustomerId INT = (SELECT TOP 1 Id FROM UserAccounts WHERE Role = 0 ORDER BY Id);

INSERT INTO Orders (UserAccountId, OrderDate, TotalAmount, Status, ShippingAddress, PhoneNumber)
VALUES
    (@CustomerId, DATEADD(DAY, 0, CAST(GETUTCDATE() AS date)) + CAST('09:00:00' AS time), 24850000, N'Paid', N'123 Demo Street, Ho Chi Minh City', N'0900000000'),
    (@CustomerId, DATEADD(DAY, -1, CAST(GETUTCDATE() AS date)) + CAST('10:00:00' AS time), 25700000, N'Paid', N'123 Demo Street, Ho Chi Minh City', N'0900000000'),
    (@CustomerId, DATEADD(DAY, -2, CAST(GETUTCDATE() AS date)) + CAST('11:00:00' AS time), 29100000, N'Paid', N'123 Demo Street, Ho Chi Minh City', N'0900000000'),
    (@CustomerId, DATEADD(DAY, -3, CAST(GETUTCDATE() AS date)) + CAST('12:00:00' AS time), 29950000, N'Paid', N'123 Demo Street, Ho Chi Minh City', N'0900000000'),
    (@CustomerId, DATEADD(DAY, -4, CAST(GETUTCDATE() AS date)) + CAST('13:00:00' AS time), 33350000, N'Paid', N'123 Demo Street, Ho Chi Minh City', N'0900000000'),
    (@CustomerId, DATEADD(DAY, -5, CAST(GETUTCDATE() AS date)) + CAST('14:00:00' AS time), 34200000, N'Paid', N'123 Demo Street, Ho Chi Minh City', N'0900000000'),
    (@CustomerId, DATEADD(DAY, -6, CAST(GETUTCDATE() AS date)) + CAST('15:00:00' AS time), 37600000, N'Paid', N'123 Demo Street, Ho Chi Minh City', N'0900000000');
GO

INSERT INTO OrderItems (OrderId, ProductId, ProductName, UnitPrice, Quantity, LineTotal)
VALUES
    (1, 1, N'Acer Nitro 5', 12000000, 1, 12000000),
    (1, 2, N'Asus TUF F15', 12850000, 1, 12850000),

    (2, 2, N'Asus TUF F15', 12850000, 2, 25700000),

    (3, 3, N'Dell G15', 13700000, 1, 13700000),
    (3, 4, N'HP Victus 16', 14550000, 1, 14550000),

    (4, 4, N'HP Victus 16', 14550000, 2, 29100000),
    (4, 5, N'Lenovo Legion 5', 15400000, 1, 15400000),

    (5, 5, N'Lenovo Legion 5', 15400000, 1, 15400000),
    (5, 6, N'MSI Katana', 16250000, 1, 16250000),

    (6, 6, N'MSI Katana', 16250000, 2, 32500000),
    (6, 7, N'MacBook Air M2', 17100000, 1, 17100000),

    (7, 7, N'MacBook Air M2', 17100000, 1, 17100000),
    (7, 8, N'Dell Inspiron 15', 17950000, 1, 17950000);
GO

SELECT
    (SELECT COUNT(*) FROM UserAccounts) AS TotalUsers,
    (SELECT COUNT(*) FROM Categories) AS TotalCategories,
    (SELECT COUNT(*) FROM Products) AS TotalProducts,
    (SELECT COUNT(*) FROM Orders WHERE Status = N'Paid') AS TotalPaidOrders;
GO
