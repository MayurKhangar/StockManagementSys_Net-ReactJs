using Microsoft.EntityFrameworkCore;
using SmartStock.Domain.Entities;
using SmartStock.Domain.Enums;

namespace SmartStock.Infrastructure.Data;

public static class AppDbContextSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Role { Name = RoleType.Admin },
                new Role { Name = RoleType.StoreManager },
                new Role { Name = RoleType.Customer }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var adminRole = await context.Roles.FirstAsync(r => r.Name == RoleType.Admin);
            var customerRole = await context.Roles.FirstAsync(r => r.Name == RoleType.Customer);

            context.Users.AddRange(
                new User
                {
                    FullName = "System Administrator",
                    Email = "admin@smartstock.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    RoleId = adminRole.Id,
                    IsActive = true
                },
                new User
                {
                    FullName = "Demo Customer",
                    Email = "customer@smartstock.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                    RoleId = customerRole.Id,
                    IsActive = true
                }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category { Name = "Electronics", Description = "Electronic devices and accessories" },
                new Category { Name = "Groceries", Description = "Daily grocery items" },
                new Category { Name = "Stationery", Description = "Office and school supplies" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Suppliers.AnyAsync())
        {
            context.Suppliers.AddRange(
                new Supplier { Name = "Acme Distributors", ContactPerson = "John Doe", Email = "john@acme.com", PhoneNumber = "9999999999", Address = "123 Market St" },
                new Supplier { Name = "Global Traders", ContactPerson = "Jane Smith", Email = "jane@globaltraders.com", PhoneNumber = "8888888888", Address = "456 Trade Ave" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Products.AnyAsync())
        {
            var electronics = await context.Categories.FirstAsync(c => c.Name == "Electronics");
            var groceries = await context.Categories.FirstAsync(c => c.Name == "Groceries");
            var stationery = await context.Categories.FirstAsync(c => c.Name == "Stationery");
            var acme = await context.Suppliers.FirstAsync(s => s.Name == "Acme Distributors");
            var global = await context.Suppliers.FirstAsync(s => s.Name == "Global Traders");

            context.Products.AddRange(
                new Product { Sku = "ELEC-0001", Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Price = 799m, CostPrice = 500m, StockQuantity = 50, LowStockThreshold = 10, CategoryId = electronics.Id, SupplierId = acme.Id },
                new Product { Sku = "ELEC-0002", Name = "USB-C Charger", Description = "65W fast charger", Price = 1299m, CostPrice = 850m, StockQuantity = 40, LowStockThreshold = 10, CategoryId = electronics.Id, SupplierId = acme.Id },
                new Product { Sku = "ELEC-0003", Name = "Bluetooth Headphones", Description = "Over-ear noise cancelling", Price = 2999m, CostPrice = 2000m, StockQuantity = 25, LowStockThreshold = 5, CategoryId = electronics.Id, SupplierId = global.Id },
                new Product { Sku = "GROC-0001", Name = "Basmati Rice 5kg", Description = "Premium basmati rice", Price = 650m, CostPrice = 480m, StockQuantity = 100, LowStockThreshold = 20, CategoryId = groceries.Id, SupplierId = global.Id },
                new Product { Sku = "GROC-0002", Name = "Sunflower Oil 1L", Description = "Refined sunflower oil", Price = 180m, CostPrice = 130m, StockQuantity = 120, LowStockThreshold = 30, CategoryId = groceries.Id, SupplierId = global.Id },
                new Product { Sku = "STAT-0001", Name = "A4 Notebook", Description = "200 pages ruled notebook", Price = 60m, CostPrice = 35m, StockQuantity = 200, LowStockThreshold = 40, CategoryId = stationery.Id, SupplierId = acme.Id },
                new Product { Sku = "STAT-0002", Name = "Gel Pen Pack (10)", Description = "Smooth writing gel pens", Price = 120m, CostPrice = 70m, StockQuantity = 8, LowStockThreshold = 15, CategoryId = stationery.Id, SupplierId = acme.Id }
            );
            await context.SaveChangesAsync();
        }
    }
}
