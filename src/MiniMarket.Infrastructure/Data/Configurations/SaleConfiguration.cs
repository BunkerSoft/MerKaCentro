using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniMarket.Domain.Entities;

namespace MiniMarket.Infrastructure.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Number)
            .IsRequired()
            .HasMaxLength(20);

        builder.OwnsOne(s => s.Subtotal, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Subtotal")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("SubtotalCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN");
        });

        builder.OwnsOne(s => s.Discount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Discount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("DiscountCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN");
        });

        builder.OwnsOne(s => s.Tax, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Tax")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("TaxCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN");
        });

        builder.OwnsOne(s => s.Total, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Total")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("TotalCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN");
        });

        builder.OwnsOne(s => s.AmountPaid, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("AmountPaid")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("AmountPaidCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN");
        });

        builder.OwnsOne(s => s.Change, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Change")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("ChangeCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN");
        });

        builder.Property(s => s.Notes)
            .HasMaxLength(500);

        builder.HasIndex(s => s.Number)
            .IsUnique();

        builder.HasIndex(s => s.CreatedAt);

        builder.HasOne(s => s.CashRegister)
            .WithMany(cr => cr.Sales)
            .HasForeignKey(s => s.CashRegisterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Payments)
            .WithOne()
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.OwnsOne(i => i.Quantity, qty =>
        {
            qty.Property(q => q.Value)
                .HasColumnName("Quantity")
                .HasPrecision(18, 3);
        });

        builder.OwnsOne(i => i.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitPrice")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("UnitPriceCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(i => i.Discount, pct =>
        {
            pct.Property(p => p.Value)
                .HasColumnName("Discount")
                .HasPrecision(5, 2);
        });

        builder.OwnsOne(i => i.Total, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Total")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("TotalCurrency")
                .HasMaxLength(3);
        });
    }
}

public class SalePaymentConfiguration : IEntityTypeConfiguration<SalePayment>
{
    public void Configure(EntityTypeBuilder<SalePayment> builder)
    {
        builder.ToTable("SalePayments");

        builder.HasKey(p => p.Id);

        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });

        builder.Property(p => p.Reference)
            .HasMaxLength(100);
    }
}
