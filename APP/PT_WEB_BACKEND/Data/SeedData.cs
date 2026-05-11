using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PT_WEB.Models;

namespace PT_WEB.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureCreatedAsync();

        // Keep existing databases compatible after removing stock management.
        await context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('Products', 'StockQuantity') IS NOT NULL
            BEGIN
                ALTER TABLE Products DROP COLUMN StockQuantity;
            END
            """);

        if (!await context.UserAccounts.AnyAsync())
        {
            var passwordHasher = new PasswordHasher<UserAccount>();

            var admin = new UserAccount
            {
                FullName = "Admin User",
                Email = "admin@laptopstore.com",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };
            admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin@123");

            var customerOne = new UserAccount
            {
                FullName = "Nguyen Van A",
                Email = "customer1@gmail.com",
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow
            };
            customerOne.PasswordHash = passwordHasher.HashPassword(customerOne, "Customer@123");

            var customerTwo = new UserAccount
            {
                FullName = "Tran Thi B",
                Email = "customer2@gmail.com",
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow
            };
            customerTwo.PasswordHash = passwordHasher.HashPassword(customerTwo, "Customer@123");

            context.UserAccounts.AddRange(admin, customerOne, customerTwo);

            var categories = new List<Category>
            {
                new() { Name = "Gaming", Description = "Gaming laptops" },
                new() { Name = "Office", Description = "Office laptops" },
                new() { Name = "Ultrabook", Description = "Thin and light laptops" },
                new() { Name = "Student", Description = "Affordable laptops for students" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            var products = new List<Product>();
            var imageUrlsByBrand = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Acer"] = "https://images.pexels.com/photos/18105/pexels-photo.jpg",
                ["Asus"] = "https://images.pexels.com/photos/1229861/pexels-photo-1229861.jpeg",
                ["Dell"] = "https://images.pexels.com/photos/205421/pexels-photo-205421.jpeg",
                ["HP"] = "https://images.pexels.com/photos/7974/pexels-photo.jpg",
                ["Lenovo"] = "https://images.pexels.com/photos/374074/pexels-photo-374074.jpeg",
                ["MSI"] = "https://images.pexels.com/photos/1181671/pexels-photo-1181671.jpeg",
                ["MacBook"] = "https://images.pexels.com/photos/1779487/pexels-photo-1779487.jpeg",
                ["Gigabyte"] = "https://images.pexels.com/photos/303383/pexels-photo-303383.jpeg",
                ["Huawei"] = "https://images.pexels.com/photos/2148216/pexels-photo-2148216.jpeg",
                ["LG"] = "https://images.pexels.com/photos/267350/pexels-photo-267350.jpeg",
                ["Razer"] = "https://images.pexels.com/photos/109371/pexels-photo-109371.jpeg"
            };
            var productNames = new[]
            {
                "Acer Nitro 5", "Asus TUF F15", "Dell G15", "HP Victus 16", "Lenovo Legion 5",
                "MSI Katana", "MacBook Air M2", "Dell Inspiron 15", "HP Pavilion 14", "Asus VivoBook 15",
                "Lenovo IdeaPad Slim", "Acer Aspire 7", "Gigabyte G5", "MSI Modern 14", "Dell XPS 13",
                "HP Envy 13", "Asus Zenbook 14", "Lenovo Yoga Slim 7", "Acer Swift 3", "Huawei MateBook D14",
                "LG Gram 16", "Razer Blade 15", "Asus ROG Strix G16", "Lenovo LOQ 15", "MSI Thin GF63",
                "Dell Latitude 5440", "HP ProBook 450", "Acer TravelMate", "Asus ExpertBook", "Lenovo ThinkBook 14"
            };

            for (var index = 0; index < productNames.Length; index++)
            {
                var category = categories[index % categories.Count];
                var brand = productNames[index].Split(' ')[0];
                products.Add(new Product
                {
                    Name = productNames[index],
                    Brand = brand,
                    Description = $"{productNames[index]} is a good laptop for study, work, and daily use.",
                    Price = 12000000 + index * 850000,
                    ImageUrl = imageUrlsByBrand.GetValueOrDefault(brand, "https://images.pexels.com/photos/250459/pexels-photo-250459.jpeg"),
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }

        await SeedRevenueDemoDataAsync(context);
    }

    private static async Task SeedRevenueDemoDataAsync(ApplicationDbContext context)
    {
        var hasPaidOrders = await context.Orders.AnyAsync(order => order.Status == OrderStatus.Paid);
        if (hasPaidOrders)
        {
            return;
        }

        var customer = await context.UserAccounts
            .Where(user => user.Role == UserRole.Customer)
            .OrderBy(user => user.Id)
            .FirstOrDefaultAsync();

        var products = await context.Products
            .Where(product => product.IsActive)
            .OrderBy(product => product.Id)
            .Take(10)
            .ToListAsync();

        if (customer is null || products.Count < 2)
        {
            return;
        }

        var orders = new List<Order>();
        var today = DateTime.UtcNow.Date;

        for (var dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            var orderDate = today.AddDays(-dayOffset).AddHours(9 + dayOffset);
            var firstProduct = products[dayOffset % products.Count];
            var secondProduct = products[(dayOffset + 1) % products.Count];

            var firstQty = 1 + (dayOffset % 2);
            var secondQty = 1;

            var firstLineTotal = firstProduct.Price * firstQty;
            var secondLineTotal = secondProduct.Price * secondQty;
            var orderTotal = firstLineTotal + secondLineTotal;

            var order = new Order
            {
                UserAccountId = customer.Id,
                OrderDate = orderDate,
                Status = OrderStatus.Paid,
                ShippingAddress = "123 Demo Street, Ho Chi Minh City",
                PhoneNumber = "0900000000",
                TotalAmount = orderTotal,
                OrderItems = new List<OrderItem>
                {
                    new()
                    {
                        ProductId = firstProduct.Id,
                        ProductName = firstProduct.Name,
                        UnitPrice = firstProduct.Price,
                        Quantity = firstQty,
                        LineTotal = firstLineTotal
                    },
                    new()
                    {
                        ProductId = secondProduct.Id,
                        ProductName = secondProduct.Name,
                        UnitPrice = secondProduct.Price,
                        Quantity = secondQty,
                        LineTotal = secondLineTotal
                    }
                }
            };

            orders.Add(order);
        }

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();
    }
}