using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lab.Orders.Application.Abstractions;

namespace Lab.Orders.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");

        builder.HasKey(r => r.Key);
        builder.Property(r => r.Key)
            .HasMaxLength(200);

        builder.Property(r => r.PayloadHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.OrderId)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();
    }
}
