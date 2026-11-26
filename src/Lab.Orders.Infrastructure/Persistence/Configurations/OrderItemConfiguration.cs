using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lab.Orders.Domain.Orders;
using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.Property<int>("Id")
            .ValueGeneratedOnAdd();
        builder.HasKey("Id");

        builder.Property<Guid>("OrderId")
            .IsRequired();

        builder.Property(i => i.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.Discount)
            .HasConversion(new DiscountValueConverter())
            .HasMaxLength(100);

        builder.Property(i => i.BasePrice)
            .HasConversion(m => m.Amount, a => new Money(a))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.LineTotal)
            .HasConversion(m => m.Amount, a => new Money(a))
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
