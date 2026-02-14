using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Enums;
using MerkaCentro.Domain.Security;
using MerkaCentro.Domain.ValueObjects;

namespace MerkaCentro.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MerkaCentroDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MerkaCentroDbContext>>();

        try
        {
            await SeedUsersAsync(context, logger);
            await SeedCategoriesAsync(context, logger);
            await SeedExpenseCategoriesAsync(context, logger);
            await SeedSuppliersAsync(context, logger);
            await SeedProductsAsync(context, logger);

            logger.LogInformation("Datos semilla insertados correctamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al insertar datos semilla");
            throw;
        }
    }

    private static async Task SeedUsersAsync(MerkaCentroDbContext context, ILogger logger)
    {
        if (await context.Users.AnyAsync())
            return;

        var users = new[]
        {
            User.Create("admin", HashPassword("Admin123!"), "Administrador del Sistema", UserRole.Admin),
            User.Create("cajero", HashPassword("Cajero123!"), "Cajero Principal", UserRole.Cashier),
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
        logger.LogInformation("Usuarios creados: admin, cajero");
    }

    private static async Task SeedCategoriesAsync(MerkaCentroDbContext context, ILogger logger)
    {
        if (await context.Categories.AnyAsync())
            return;

        var categories = new[]
        {
            Category.Create("Bebidas", "Agua, gaseosas, jugos y bebidas en general"),
            Category.Create("Lacteos", "Leche, yogurt, queso y derivados"),
            Category.Create("Panaderia", "Pan, galletas, tortas y productos de panaderia"),
            Category.Create("Snacks y Golosinas", "Papas fritas, chocolates, caramelos y dulces"),
            Category.Create("Abarrotes", "Arroz, azucar, aceite, fideos y productos basicos"),
            Category.Create("Limpieza", "Detergente, jabon, lejia y productos de limpieza"),
            Category.Create("Cuidado Personal", "Shampoo, pasta dental, jabon y cuidado personal"),
            Category.Create("Frutas y Verduras", "Frutas frescas y verduras"),
            Category.Create("Carnes y Embutidos", "Pollo, carne, jamon y embutidos"),
            Category.Create("Congelados", "Helados, productos congelados y refrigerados"),
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
        logger.LogInformation("Categorias creadas: {Count}", categories.Length);
    }

    private static async Task SeedExpenseCategoriesAsync(MerkaCentroDbContext context, ILogger logger)
    {
        if (await context.ExpenseCategories.AnyAsync())
            return;

        var expenseCategories = new[]
        {
            ExpenseCategory.Create("Alquiler", "Pago mensual de alquiler del local"),
            ExpenseCategory.Create("Servicios Basicos", "Luz, agua, internet y telefono"),
            ExpenseCategory.Create("Sueldos", "Pago de sueldos y salarios del personal"),
            ExpenseCategory.Create("Transporte", "Gastos de transporte y delivery"),
            ExpenseCategory.Create("Mantenimiento", "Reparaciones y mantenimiento del local"),
            ExpenseCategory.Create("Impuestos", "Pagos de impuestos y tributos"),
            ExpenseCategory.Create("Otros", "Gastos varios no categorizados"),
        };

        context.ExpenseCategories.AddRange(expenseCategories);
        await context.SaveChangesAsync();
        logger.LogInformation("Categorias de gasto creadas: {Count}", expenseCategories.Length);
    }

    private static async Task SeedSuppliersAsync(MerkaCentroDbContext context, ILogger logger)
    {
        if (await context.Suppliers.AnyAsync())
            return;

        var suppliers = new[]
        {
            Supplier.Create(
                "Distribuidora Lima SAC",
                "Distribuidora Lima S.A.C.",
                address: Address.Create("Av. Argentina 1234", "Cercado de Lima", "Lima")),
            Supplier.Create(
                "Productos del Norte EIRL",
                "Productos del Norte E.I.R.L.",
                address: Address.Create("Jr. Comercio 567", "Trujillo", "La Libertad")),
            Supplier.Create(
                "Alimentos Frescos SAC",
                "Alimentos Frescos S.A.C.",
                address: Address.Create("Calle Los Olivos 890", "Los Olivos", "Lima")),
        };

        context.Suppliers.AddRange(suppliers);
        await context.SaveChangesAsync();
        logger.LogInformation("Proveedores creados: {Count}", suppliers.Length);
    }

    private static async Task SeedProductsAsync(MerkaCentroDbContext context, ILogger logger)
    {
        if (await context.Products.AnyAsync())
            return;

        var categories = await context.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);

        var products = new List<(string Code, string Name, string CatName, decimal PurchasePrice, decimal SalePrice, string Unit)>
        {
            ("BEB001", "Agua Mineral 500ml", "Bebidas", 0.50m, 1.00m, "Unidad"),
            ("BEB002", "Gaseosa Cola 500ml", "Bebidas", 1.20m, 2.50m, "Unidad"),
            ("BEB003", "Jugo de Naranja 1L", "Bebidas", 2.00m, 3.50m, "Unidad"),
            ("BEB004", "Gaseosa Limon 2L", "Bebidas", 2.50m, 5.00m, "Unidad"),
            ("BEB005", "Agua Mineral 2.5L", "Bebidas", 1.00m, 2.00m, "Unidad"),
            ("LAC001", "Leche Entera 1L", "Lacteos", 3.00m, 4.50m, "Unidad"),
            ("LAC002", "Yogurt Natural 1L", "Lacteos", 3.50m, 5.50m, "Unidad"),
            ("LAC003", "Queso Fresco 500g", "Lacteos", 6.00m, 9.00m, "Unidad"),
            ("LAC004", "Mantequilla 200g", "Lacteos", 3.00m, 4.50m, "Unidad"),
            ("PAN001", "Pan Frances (unidad)", "Panaderia", 0.10m, 0.20m, "Unidad"),
            ("PAN002", "Pan Integral (unidad)", "Panaderia", 0.15m, 0.30m, "Unidad"),
            ("PAN003", "Galletas Soda (paquete)", "Panaderia", 1.00m, 1.80m, "Paquete"),
            ("SNK001", "Papas Fritas 150g", "Snacks y Golosinas", 2.00m, 3.50m, "Unidad"),
            ("SNK002", "Chocolate con Leche 30g", "Snacks y Golosinas", 0.80m, 1.50m, "Unidad"),
            ("SNK003", "Caramelos Surtidos 100g", "Snacks y Golosinas", 1.00m, 2.00m, "Bolsa"),
            ("ABR001", "Arroz Extra 1kg", "Abarrotes", 3.50m, 4.80m, "Kilogramo"),
            ("ABR002", "Azucar Rubia 1kg", "Abarrotes", 2.80m, 3.80m, "Kilogramo"),
            ("ABR003", "Aceite Vegetal 1L", "Abarrotes", 5.00m, 7.50m, "Unidad"),
            ("ABR004", "Fideos Spaghetti 500g", "Abarrotes", 1.50m, 2.50m, "Unidad"),
            ("ABR005", "Atun en Conserva 170g", "Abarrotes", 3.00m, 4.50m, "Unidad"),
            ("ABR006", "Sal de Mesa 1kg", "Abarrotes", 0.80m, 1.50m, "Kilogramo"),
            ("LIM001", "Detergente en Polvo 500g", "Limpieza", 3.00m, 5.00m, "Unidad"),
            ("LIM002", "Lejia 1L", "Limpieza", 2.00m, 3.50m, "Unidad"),
            ("LIM003", "Jabon en Barra (unidad)", "Limpieza", 1.50m, 2.50m, "Unidad"),
            ("CUI001", "Shampoo 400ml", "Cuidado Personal", 8.00m, 12.00m, "Unidad"),
            ("CUI002", "Pasta Dental 100ml", "Cuidado Personal", 3.50m, 5.50m, "Unidad"),
            ("CUI003", "Papel Higienico x4", "Cuidado Personal", 3.00m, 5.00m, "Paquete"),
        };

        var now = DateTime.UtcNow;
        int count = 0;

        foreach (var p in products)
        {
            if (!categories.TryGetValue(p.CatName, out var catId))
                continue;

            var productId = Guid.NewGuid();

            // Usar SQL directo para evitar problemas con Owned Entities en EF Core 8
            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Products (Id, Code, Name, CategoryId, PurchasePrice, PurchaseCurrency, SalePrice, SaleCurrency, MinStock, CurrentStock, Unit, AllowFractions, Status, CreatedAt)
                VALUES ({0}, {1}, {2}, {3}, {4}, 'PEN', {5}, 'PEN', 5, 0, {6}, 0, 0, {7})",
                productId, p.Code, p.Name, catId, p.PurchasePrice, p.SalePrice, p.Unit, now);

            count++;
        }

        logger.LogInformation("Productos creados: {Count}", count);
    }

    private static string HashPassword(string password)
    {
        return PasswordHasher.Hash(password);
    }
}
