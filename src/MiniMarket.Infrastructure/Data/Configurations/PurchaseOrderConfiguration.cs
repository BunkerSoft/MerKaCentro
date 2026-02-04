using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniMarket.Domain.Entities;

namespace MiniMarket.Infrastructure.Data.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");

        builder.HasKey(po => po.Id);

        builder.Property(po => po.Number)
            .IsRequired()
            .HasMaxLength(20);

        builder.OwnsOne(po => po.Total, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Total")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("TotalCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("PEN");
        });

        builder.Property(po => po.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(po => po.Number)
            .IsUnique();

        builder.HasIndex(po => po.CreatedAt);

        builder.HasOne(po => po.User)
            .WithMany()
            .HasForeignKey(po => po.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(po => po.Items)
            .WithOne()
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");

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

        builder.OwnsOne(i => i.ReceivedQuantity, qty =>
        {
            qty.Property(q => q.Value)
                .HasColumnName("ReceivedQuantity")
                .HasPrecision(18, 3);
        });

        builder.OwnsOne(i => i.UnitCost, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitCost")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("UnitCostCurrency")
                .HasMaxLength(3);
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
