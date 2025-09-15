using EcommerceApi.Models;
using Microsoft.AspNetCore.Identity;

namespace EcommerceApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await SeedRolesAsync(roleManager);
        
        await SeedUsersAsync(userManager);
        
        await SeedCategoriesAsync(context);
        
        await SeedProductsAsync(context);
        
        await SeedReviewsAsync(context, userManager);
        
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Customer" };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByEmailAsync("admin@example.com") == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "Password123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        if (await userManager.FindByEmailAsync("customer@example.com") == null)
        {
            var customerUser = new ApplicationUser
            {
                UserName = "customer@example.com",
                Email = "customer@example.com",
                FirstName = "John",
                LastName = "Doe",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(customerUser, "Password123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customerUser, "Customer");
            }
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (context.Categories.Any()) return;

        var categories = new[]
        {
            new Category { Name = "Electronics", Description = "Electronic devices and gadgets", IsActive = true, SortOrder = 1 },
            new Category { Name = "Clothing", Description = "Fashion and apparel", IsActive = true, SortOrder = 2 },
            new Category { Name = "Books", Description = "Books and literature", IsActive = true, SortOrder = 3 },
            new Category { Name = "Home & Garden", Description = "Home improvement and gardening", IsActive = true, SortOrder = 4 },
            new Category { Name = "Sports", Description = "Sports and outdoor activities", IsActive = true, SortOrder = 5 }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var electronics = context.Categories.First(c => c.Name == "Electronics");
        var clothing = context.Categories.First(c => c.Name == "Clothing");

        var subCategories = new[]
        {
            new Category { Name = "Smartphones", Description = "Mobile phones and accessories", ParentCategoryId = electronics.Id, IsActive = true, SortOrder = 1 },
            new Category { Name = "Laptops", Description = "Laptops and computers", ParentCategoryId = electronics.Id, IsActive = true, SortOrder = 2 },
            new Category { Name = "Men's Clothing", Description = "Clothing for men", ParentCategoryId = clothing.Id, IsActive = true, SortOrder = 1 },
            new Category { Name = "Women's Clothing", Description = "Clothing for women", ParentCategoryId = clothing.Id, IsActive = true, SortOrder = 2 }
        };

        context.Categories.AddRange(subCategories);
    }

    private static async Task SeedProductsAsync(ApplicationDbContext context)
    {
        if (context.Products.Any()) return;

        var electronics = context.Categories.First(c => c.Name == "Electronics");
        var smartphones = context.Categories.First(c => c.Name == "Smartphones");
        var laptops = context.Categories.First(c => c.Name == "Laptops");
        var clothing = context.Categories.First(c => c.Name == "Clothing");
        var books = context.Categories.First(c => c.Name == "Books");

        var products = new[]
        {
            new Product
            {
                Name = "iPhone 15 Pro",
                Description = "Latest iPhone with advanced camera system and A17 Pro chip",
                Price = 999.99m,
                CategoryId = smartphones.Id,
                ImageUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=400",
                InStock = true,
                StockQuantity = 50,
                Tags = new[] { "smartphone", "apple", "premium" },
                IsFeatured = true,
                IsActive = true
            },
            new Product
            {
                Name = "MacBook Pro 14\"",
                Description = "Powerful laptop with M3 chip for professionals",
                Price = 1999.99m,
                CategoryId = laptops.Id,
                ImageUrl = "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=400",
                InStock = true,
                StockQuantity = 25,
                Tags = new[] { "laptop", "apple", "professional" },
                IsFeatured = true,
                IsActive = true
            },
            new Product
            {
                Name = "Samsung Galaxy S24",
                Description = "Android smartphone with excellent camera and display",
                Price = 799.99m,
                CategoryId = smartphones.Id,
                ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=400",
                InStock = true,
                StockQuantity = 40,
                Tags = new[] { "smartphone", "samsung", "android" },
                IsFeatured = false,
                IsActive = true
            },
            
            new Product
            {
                Name = "Classic White T-Shirt",
                Description = "Comfortable cotton t-shirt for everyday wear",
                Price = 29.99m,
                CategoryId = clothing.Id,
                ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=400",
                InStock = true,
                StockQuantity = 100,
                Tags = new[] { "t-shirt", "cotton", "casual" },
                IsFeatured = false,
                IsActive = true
            },
            new Product
            {
                Name = "Denim Jeans",
                Description = "Premium denim jeans with perfect fit",
                Price = 89.99m,
                CategoryId = clothing.Id,
                ImageUrl = "https://images.unsplash.com/photo-1542272604-787c3835535d?w=400",
                InStock = true,
                StockQuantity = 75,
                Tags = new[] { "jeans", "denim", "casual" },
                IsFeatured = true,
                IsActive = true
            },
            
            new Product
            {
                Name = "The Art of Programming",
                Description = "Comprehensive guide to software development best practices",
                Price = 49.99m,
                CategoryId = books.Id,
                ImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400",
                InStock = true,
                StockQuantity = 30,
                Tags = new[] { "programming", "technology", "education" },
                IsFeatured = false,
                IsActive = true
            },
            new Product
            {
                Name = "Modern Web Development",
                Description = "Learn the latest web technologies and frameworks",
                Price = 39.99m,
                CategoryId = books.Id,
                ImageUrl = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=400",
                InStock = true,
                StockQuantity = 45,
                Tags = new[] { "web development", "javascript", "react" },
                IsFeatured = true,
                IsActive = true
            },
            
            new Product
            {
                Name = "Wireless Headphones",
                Description = "High-quality wireless headphones with noise cancellation",
                Price = 199.99m,
                CategoryId = electronics.Id,
                ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400",
                InStock = true,
                StockQuantity = 60,
                Tags = new[] { "headphones", "wireless", "audio" },
                IsFeatured = true,
                IsActive = true
            }
        };

        context.Products.AddRange(products);
    }

    private static async Task SeedReviewsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.Reviews.Any()) return;

        var customer = await userManager.FindByEmailAsync("customer@example.com");
        if (customer == null) return;

        var products = context.Products.Take(3).ToList();
        if (!products.Any()) return;

        var reviews = new[]
        {
            new Review
            {
                UserId = customer.Id,
                ProductId = products[0].Id,
                Rating = 5,
                Title = "Excellent product!",
                Comment = "This product exceeded my expectations. Great quality and fast delivery.",
                IsVerifiedPurchase = true,
                HelpfulCount = 5,
                IsActive = true
            },
            new Review
            {
                UserId = customer.Id,
                ProductId = products[1].Id,
                Rating = 4,
                Title = "Good value for money",
                Comment = "Solid product with good features. Would recommend to others.",
                IsVerifiedPurchase = true,
                HelpfulCount = 3,
                IsActive = true
            },
            new Review
            {
                UserId = customer.Id,
                ProductId = products[2].Id,
                Rating = 5,
                Title = "Perfect!",
                Comment = "Exactly what I was looking for. High quality and great design.",
                IsVerifiedPurchase = false,
                HelpfulCount = 8,
                IsActive = true
            }
        };

        context.Reviews.AddRange(reviews);
    }
}
