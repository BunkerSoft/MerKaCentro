using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniMarket.Domain.Entities;

namespace MiniMarket.Infrastructure.Data.Configurations;

public class CashRegisterConfiguration : IEntityTypeConfiguration<CashRegister>
{
    public void Configure(EntityTypeBuilder<CashRegister> builder)
    {
        builder.ToTable("CashRegisters");

        builder.HasKey(cr => cr.Id);

        builder.OwnsOne(cr => cr.InitialCash, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("InitialCash")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("InitialCashCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN");
        });

        builder.OwnsOne(cr => cr.CurrentCash, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CurrentCash")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("CurrentCashCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN");
        });

        builder.OwnsOne(cr => cr.ExpectedCash, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ExpectedCash")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("ExpectedCashCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN");
        });

        builder.OwnsOne(cr => cr.FinalCash, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("FinalCash")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("FinalCashCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(cr => cr.Difference, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Difference")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("DifferenceCurrency")
                .HasMaxLength(3);
        });

        builder.Property(cr => cr.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(cr => new { cr.UserId, cr.OpenedAt });

        builder.HasOne(cr => cr.User)
            .WithMany()
            .HasForeignKey(cr => cr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(cr => cr.Movements)
            .WithOne()
            .HasForeignKey(m => m.CashRegisterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CashMovementConfiguration : IEntityTypeConfiguration<CashMovement>
{
    public void Configure(EntityTypeBuilder<CashMovement> builder)
    {
        builder.ToTable("CashMovements");

        builder.HasKey(m => m.Id);

        builder.OwnsOne(m => m.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(m => m.BalanceAfter, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("BalanceAfter")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("BalanceAfterCurrency")
                .HasMaxLength(3);
        });

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(m => new { m.CashRegisterId, m.CreatedAt });
    }
}
