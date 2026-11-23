namespace Lab.Orders.Application;

/// <summary>
/// Request for creating an order.
/// </summary>
public record CreateOrderCommand(Guid CustomerId, List<CreateOrderItemDto> Items);
