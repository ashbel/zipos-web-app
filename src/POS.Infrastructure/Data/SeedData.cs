using Microsoft.EntityFrameworkCore;
using POS.Shared.Domain;

namespace POS.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(POSDbContext context)
    {
        await context.Database.MigrateAsync();
        
        // Seed default permissions
        if (!await context.Permissions.AnyAsync())
        {
            var permissions = new List<Permission>
            {
                // Authentication permissions
                new() { Name = "users:view", Description = "View users", Module = "Authentication" },
                new() { Name = "users:create", Description = "Create users", Module = "Authentication" },
                new() { Name = "users:update", Description = "Update users", Module = "Authentication" },
                new() { Name = "users:delete", Description = "Delete users", Module = "Authentication" },
                new() { Name = "roles:manage", Description = "Manage roles", Module = "Authentication" },
                
                // Sales permissions
                new() { Name = "sales:view", Description = "View sales", Module = "Sales" },
                new() { Name = "sales:create", Description = "Create sales", Module = "Sales" },
                new() { Name = "sales:refund", Description = "Process refunds", Module = "Sales" },
                new() { Name = "sales:discount", Description = "Apply discounts", Module = "Sales" },
                
                // Inventory permissions
                new() { Name = "inventory:view", Description = "View inventory", Module = "Inventory" },
                new() { Name = "inventory:manage", Description = "Manage inventory", Module = "Inventory" },
                new() { Name = "inventory:adjust", Description = "Adjust stock levels", Module = "Inventory" },
                new() { Name = "products:manage", Description = "Manage products", Module = "Inventory" },
                
                // Customer permissions
                new() { Name = "customers:view", Description = "View customers", Module = "Customers" },
                new() { Name = "customers:manage", Description = "Manage customers", Module = "Customers" },
                
                // Branch permissions
                new() { Name = "branches:view", Description = "View branches", Module = "Branches" },
                new() { Name = "branches:manage", Description = "Manage branches", Module = "Branches" },
                
                // Reporting permissions
                new() { Name = "reports:view", Description = "View reports", Module = "Reporting" },
                new() { Name = "reports:export", Description = "Export reports", Module = "Reporting" },
                
                // Payment permissions
                new() { Name = "payments:process", Description = "Process payments", Module = "Payments" },
                new() { Name = "payments:view", Description = "View payment history", Module = "Payments" },
                
                // Recipe permissions
                new() { Name = "recipes:view", Description = "View recipes", Module = "Recipes" },
                new() { Name = "recipes:manage", Description = "Manage recipes", Module = "Recipes" },
                
                // Promotion permissions
                new() { Name = "promotions:view", Description = "View promotions", Module = "Promotions" },
                new() { Name = "promotions:manage", Description = "Manage promotions", Module = "Promotions" }
            };
            
            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }
    }
}