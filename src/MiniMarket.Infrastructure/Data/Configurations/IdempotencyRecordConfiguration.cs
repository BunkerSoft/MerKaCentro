using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniMarket.Domain.Entities;

namespace MiniMarket.Infrastructure.Data.Configurations;

public class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IdempotencyKey)
            .IsRequired();

        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique();

        builder.Property(x => x.OperationType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.RequestHash)
            .HasMaxLength(64);

        builder.Property(x => x.ResponseData);

        builder.Property(x => x.StatusCode)
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.HasIndex(x => x.ExpiresAt);
    }
}
