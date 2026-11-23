namespace Lab.Orders.Application;

/// <summary>
/// DTO for order item in create order command.
/// </summary>
public record CreateOrderItemDto(decimal UnitPrice, int Quantity, decimal DiscountPercent = 0m);
