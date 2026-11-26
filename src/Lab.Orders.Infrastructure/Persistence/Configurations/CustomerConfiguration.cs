using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lab.Orders.Domain.Customers;

namespace Lab.Orders.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.CustomerId);
        builder.Property(c => c.CustomerId).ValueGeneratedNever();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Phone)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Document)
            .HasConversion(d => d.Value, v => Document.Create(v))
            .HasMaxLength(14)
            .IsRequired();

        builder.OwnsOne(c => c.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("Street").IsRequired().HasMaxLength(200);
            address.Property(a => a.Number).HasColumnName("Number").IsRequired().HasMaxLength(50);
            address.Property(a => a.Complement).HasColumnName("Complement").HasMaxLength(100);
            address.Property(a => a.Neighborhood).HasColumnName("Neighborhood").IsRequired().HasMaxLength(100);
            address.Property(a => a.City).HasColumnName("City").IsRequired().HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("State").IsRequired().HasMaxLength(2);
            address.Property(a => a.PostalCode).HasColumnName("PostalCode").IsRequired().HasMaxLength(10);
        });
    }
}
