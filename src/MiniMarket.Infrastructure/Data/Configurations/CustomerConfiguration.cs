using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniMarket.Domain.Entities;

namespace MiniMarket.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.OwnsOne(c => c.DocumentNumber, doc =>
        {
            doc.Property(d => d.Value)
                .HasColumnName("DocumentNumber")
                .HasMaxLength(20);
            doc.Property(d => d.Type)
                .HasColumnName("DocumentType");
        });

        builder.OwnsOne(c => c.Phone, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("Phone")
                .HasMaxLength(20);
        });

        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(100);
        });

        builder.OwnsOne(c => c.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("Street")
                .HasMaxLength(200);
            address.Property(a => a.District)
                .HasColumnName("District")
                .HasMaxLength(100);
            address.Property(a => a.City)
                .HasColumnName("City")
                .HasMaxLength(100);
            address.Property(a => a.Reference)
                .HasColumnName("AddressReference")
                .HasMaxLength(200);
        });

        builder.OwnsOne(c => c.CreditLimit, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CreditLimit")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("CreditCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN");
        });

        builder.OwnsOne(c => c.CurrentDebt, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CurrentDebt")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("DebtCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN");
        });

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);

        builder.HasMany(c => c.Sales)
            .WithOne(s => s.Customer)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.CreditPayments)
            .WithOne()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CreditPaymentConfiguration : IEntityTypeConfiguration<CreditPayment>
{
    public void Configure(EntityTypeBuilder<CreditPayment> builder)
    {
        builder.ToTable("CreditPayments");

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

        builder.Property(p => p.Notes)
            .HasMaxLength(500);

        builder.HasIndex(p => new { p.CustomerId, p.CreatedAt });
    }
}
