using Lab.Orders.Application.Common;
using Lab.Orders.Application.Abstractions;
using Lab.Orders.Domain.Products;
using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Application.Products;

public sealed record RegisterProductCommand(string? Sku, string? Description, decimal BasePrice);

public sealed record RegisterProductResult(Guid ProductId, string Sku, string Description, decimal BasePrice);

public sealed class RegisterProduct
{
    private readonly IProductRepository _productRepository;

    public RegisterProduct(IProductRepository productRepository) => _productRepository = productRepository;

    public async Task<Result<RegisterProductResult>> ExecuteAsync(
        RegisterProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Sku))
            return Result<RegisterProductResult>.Validation("SkuRequired", "Product SKU is required.");

        if (string.IsNullOrWhiteSpace(command.Description))
            return Result<RegisterProductResult>.Validation("DescriptionRequired", "Product description is required.");

        if (command.BasePrice < 0m)
            return Result<RegisterProductResult>.Validation("NegativePrice", "Product base price cannot be negative.");

        Product product;

        try
        {
            product = Product.Create(Guid.NewGuid(), command.Sku, command.Description, new Money(command.BasePrice));
        }
        catch (DomainException exception)
        {
            return Result<RegisterProductResult>.Validation("InvalidProduct", exception.Message);
        }

        await _productRepository.AddAsync(product, cancellationToken);

        return Result<RegisterProductResult>.Ok(new RegisterProductResult(
            product.ProductId,
            product.Sku,
            product.Description,
            product.BasePrice.Amount));
    }
}
