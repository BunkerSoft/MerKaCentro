using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniMarket.Domain.Entities;

namespace MiniMarket.Infrastructure.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.BusinessName)
            .HasMaxLength(200);

        builder.OwnsOne(s => s.Ruc, doc =>
        {
            doc.Property(d => d.Value)
                .HasColumnName("Ruc")
                .HasMaxLength(11);
            doc.Property(d => d.Type)
                .HasColumnName("DocumentType");
        });

        builder.OwnsOne(s => s.Phone, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("Phone")
                .HasMaxLength(20);
        });

        builder.OwnsOne(s => s.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(100);
        });

        builder.OwnsOne(s => s.Address, address =>
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

        builder.Property(s => s.ContactPerson)
            .HasMaxLength(100);

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        builder.HasMany(s => s.PurchaseOrders)
            .WithOne(po => po.Supplier)
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
