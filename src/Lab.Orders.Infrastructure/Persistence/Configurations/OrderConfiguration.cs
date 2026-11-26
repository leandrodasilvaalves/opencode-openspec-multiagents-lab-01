using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lab.Orders.Domain.Delivery;
using Lab.Orders.Domain.Orders;
using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    private static readonly Func<decimal, DeliveryFee> DeliveryFeeFactory = CreateDeliveryFeeFactory();

    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.OrderId);
        builder.Property(o => o.OrderId).ValueGeneratedNever();

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.OrderItems)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(o => o.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.Discount)
            .HasConversion(new DiscountValueConverter())
            .HasMaxLength(100);

        builder.Property(o => o.DistanceKm)
            .HasConversion(d => d.Km, km => new Distance(km))
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(o => o.DeliveryFee)
            .HasConversion(f => f.Amount.Amount, a => DeliveryFeeFactory(a))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.Subtotal)
            .HasConversion(m => m.Amount, a => new Money(a))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.OrderDiscount)
            .HasConversion(m => m.Amount, a => new Money(a))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.Total)
            .HasConversion(m => m.Amount, a => new Money(a))
            .HasPrecision(18, 2)
            .IsRequired();
    }

    private static Func<decimal, DeliveryFee> CreateDeliveryFeeFactory()
    {
        var ctor = typeof(DeliveryFee).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(Money)],
            null)!;
        var param = Expression.Parameter(typeof(decimal), "amount");
        var moneyCtor = typeof(Money).GetConstructor([typeof(decimal)])!;
        var newMoney = Expression.New(moneyCtor, param);
        var newDeliveryFee = Expression.New(ctor, newMoney);
        return Expression.Lambda<Func<decimal, DeliveryFee>>(newDeliveryFee, param).Compile();
    }
}
