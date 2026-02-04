using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniMarket.Domain.Entities;

namespace MiniMarket.Infrastructure.Data.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("Alerts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Type)
            .IsRequired();

        builder.Property(a => a.Severity)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired();

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.EntityType)
            .HasMaxLength(100);

        builder.HasOne(a => a.AcknowledgedByUser)
            .WithMany()
            .HasForeignKey(a => a.AcknowledgedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.Type);
        builder.HasIndex(a => a.Severity);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.CreatedAt);
    }
}
