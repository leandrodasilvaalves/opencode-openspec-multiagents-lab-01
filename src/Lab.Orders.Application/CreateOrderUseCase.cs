namespace Lab.Orders.Application;

/// <summary>
/// Use case for creating an order.
/// </summary>
public class CreateOrderUseCase
{
    private readonly ICustomersRepository _customersRepository;
    private readonly IOrdersRepository _ordersRepository;

    public CreateOrderUseCase(ICustomersRepository customersRepository, IOrdersRepository ordersRepository)
    {
        _customersRepository = customersRepository;
        _ordersRepository = ordersRepository;
    }

    public async Task<ApplicationResult> ExecuteAsync(CreateOrderCommand command)
    {
        // 1. Carregar cliente
        var customer = await _customersRepository.GetByIdAsync(command.CustomerId);
        if (customer is null)
        {
            return new ApplicationResult(false, error: "Cliente não encontrado.");
        }

        // 2. Criar agregado Order
        var items = command.Items.Select(i => new Lab.Orders.Domain.OrderItem(i.UnitPrice, i.Quantity, i.DiscountPercent)).ToList();
        var order = new Lab.Orders.Domain.Order(customer.Id, items.ToArray());

        // 3. Validar pedidos
        var validationResult = order.Validate();
        if (validationResult != Lab.Orders.Domain.OrderStatus.Valid)
        {
            return new ApplicationResult(false, error: $"Pedido inválido: status {validationResult}");
        }

        // 4. Verificar limite de crédito
        if (order.GetTotal() > customer.CreditLimit)
        {
            return new ApplicationResult(false, error: $"Limite de crédito excedido. Total: {order.GetTotal()}, Limite: {customer.CreditLimit}");
        }

        // 5. Persistir
        await _ordersRepository.AddAsync(order);
        await _ordersRepository.SaveChangesAsync();

        return new ApplicationResult(true, order);
    }
}

/// <summary>
/// Resultado do aplicativo.
/// </summary>
public class ApplicationResult
{
    public bool Success { get; }
    public string? Error { get; set; }
    public Lab.Orders.Domain.Order? Order { get; }

    public ApplicationResult(bool success, Lab.Orders.Domain.Order? order = null, string? error = null)
    {
        Success = success;
        Order = order;
        Error = error;
    }
}
