using Microsoft.EntityFrameworkCore;
using Lab.Orders.Domain.Products;
using Lab.Orders.Application.Abstractions;
using Lab.Orders.Infrastructure.Persistence;

namespace Lab.Orders.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly OrdersDbContext _context;

    public ProductRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var trimmed = sku.Trim();
        return await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Sku == trimmed, cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
    {
        var trimmed = sku.Trim();
        return await _context.Products.AnyAsync(p => p.Sku == trimmed, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
