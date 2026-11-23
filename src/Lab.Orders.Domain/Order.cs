namespace Lab.Orders.Domain;

/// <summary>
/// Agregado raiz do domínio de Pedidos.
/// </summary>
public sealed class Order
{
    public Guid Id { get; }

    public Guid CustomerId { get; }

    private List<OrderItem> _items;

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAt { get; }

    public decimal Total { get; private set; }

    private Order()
    {
        // EF Core construtor
        Status = OrderStatus.Created;
        CreatedAt = DateTime.UtcNow;
        _items = new List<OrderItem>();
        Total = 0m;
    }

    public Order(Guid customerId, params OrderItem[] items)
    {
        if (items is null or { Length: 0 })
        {
            throw new ArgumentException("Pedido deve conter ao menos um item.", nameof(items));
        }

        CustomerId = customerId;
        _items = new List<OrderItem>(items);
        Status = OrderStatus.Created;
        CreatedAt = DateTime.UtcNow;
        Total = CalculateTotal();
    }

    public void AddItem(OrderItem item)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        _items.Add(item);
        Total = CalculateTotal();
    }

    public OrderStatus Validate()
    {
        if (_items.Count > 0)
        {
            foreach (var item in _items)
            {
                if (item.UnitPrice <= 0m)
                {
                    return OrderStatus.InvalidItemPrice;
                }
                if (item.Quantity <= 0)
                {
                    return OrderStatus.InvalidItemQuantity;
                }
            }
        }

        // Validation passed - update status to Valid
        Status = OrderStatus.Valid;
        return Status;
    }

    private decimal CalculateTotal()
    {
        decimal sum = 0m;
        foreach (var item in _items)
        {
            sum += item.LineTotal;
        }
        return sum;
    }

    public decimal GetTotal()
    {
        return Total;
    }
}
