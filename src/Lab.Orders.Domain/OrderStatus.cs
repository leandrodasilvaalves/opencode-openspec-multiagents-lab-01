namespace Lab.Orders.Domain;

/// <summary>
/// Status do pedido.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Pedido criado, ainda não validado.
    /// </summary>
    Created,

    /// <summary>
    /// Pedido inválido.
    /// </summary>
    Invalid,

    /// <summary>
    /// Pedido validado com sucesso.
    /// </summary>
    Valid,

    /// <summary>
    /// Item de preço inválido.
    /// </summary>
    InvalidItemPrice,

    /// <summary>
    /// Quantidade de item inválida.
    /// </summary>
    InvalidItemQuantity
}
