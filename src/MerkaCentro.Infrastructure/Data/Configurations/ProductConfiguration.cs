using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.ValueObjects;

namespace MerkaCentro.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.OwnsOne(p => p.Barcode, barcode =>
        {
            barcode.Property(b => b.Value)
                .HasColumnName("Barcode")
                .HasMaxLength(14);
        });

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.OwnsOne(p => p.PurchasePrice, price =>
        {
            price.Property(m => m.Amount)
                .HasColumnName("PurchasePrice")
                .HasPrecision(18, 2)
                .IsRequired();
            price.Property(m => m.Currency)
                .HasColumnName("PurchaseCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN")
                .IsRequired();
        });

        builder.OwnsOne(p => p.SalePrice, price =>
        {
            price.Property(m => m.Amount)
                .HasColumnName("SalePrice")
                .HasPrecision(18, 2)
                .IsRequired();
            price.Property(m => m.Currency)
                .HasColumnName("SaleCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN")
                .IsRequired();
        });

        builder.OwnsOne(p => p.MinStock, qty =>
        {
            qty.Property(q => q.Value)
                .HasColumnName("MinStock")
                .HasPrecision(18, 3);
        });

        builder.OwnsOne(p => p.CurrentStock, qty =>
        {
            qty.Property(q => q.Value)
                .HasColumnName("CurrentStock")
                .HasPrecision(18, 3);
        });

        builder.Property(p => p.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(p => p.Code)
            .IsUnique();

        // Disabled: ProductPriceHistory causes EF Core Owned Entity tracking conflicts
        // builder.HasMany(p => p.PriceHistory)
        //     .WithOne()
        //     .HasForeignKey(h => h.ProductId)
        //     .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.StockMovements)
            .WithOne()
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// Disabled: ProductPriceHistory causes EF Core Owned Entity tracking conflicts
// public class ProductPriceHistoryConfiguration : IEntityTypeConfiguration<ProductPriceHistory>
// {
//     public void Configure(EntityTypeBuilder<ProductPriceHistory> builder)
//     {
//         builder.ToTable("ProductPriceHistories");
//
//         builder.HasKey(h => h.Id);
//
//         builder.OwnsOne(h => h.PurchasePrice, price =>
//         {
//             price.Property(m => m.Amount)
//                 .HasColumnName("PurchasePrice")
//                 .HasPrecision(18, 2);
//             price.Property(m => m.Currency)
//                 .HasColumnName("PurchaseCurrency")
//                 .HasMaxLength(3)
//                 .HasDefaultValue("PEN");
//         });
//
//         builder.OwnsOne(h => h.SalePrice, price =>
//         {
//             price.Property(m => m.Amount)
//                 .HasColumnName("SalePrice")
//                 .HasPrecision(18, 2);
//             price.Property(m => m.Currency)
//                 .HasColumnName("SaleCurrency")
//                 .HasMaxLength(3)
//                 .HasDefaultValue("PEN");
//         });
//
//         builder.HasIndex(h => new { h.ProductId, h.EffectiveDate });
//     }
// }

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(m => m.Id);

        builder.OwnsOne(m => m.Quantity, qty =>
        {
            qty.Property(q => q.Value)
                .HasColumnName("Quantity")
                .HasPrecision(18, 3);
        });

        builder.OwnsOne(m => m.StockAfter, qty =>
        {
            qty.Property(q => q.Value)
                .HasColumnName("StockAfter")
                .HasPrecision(18, 3);
        });

        builder.Property(m => m.Reference)
            .HasMaxLength(100);

        builder.Property(m => m.Notes)
            .HasMaxLength(500);

        builder.HasIndex(m => new { m.ProductId, m.CreatedAt });
    }
}
