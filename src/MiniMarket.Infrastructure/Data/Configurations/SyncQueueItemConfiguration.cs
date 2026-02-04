using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniMarket.Domain.Entities;

namespace MiniMarket.Infrastructure.Data.Configurations;

public class SyncQueueItemConfiguration : IEntityTypeConfiguration<SyncQueueItem>
{
    public void Configure(EntityTypeBuilder<SyncQueueItem> builder)
    {
        builder.ToTable("SyncQueue");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SyncId)
            .IsRequired();

        builder.HasIndex(x => x.SyncId)
            .IsUnique();

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.EntityId)
            .IsRequired();

        builder.Property(x => x.Operation)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
