namespace Lab.Orders.Domain;

/// <summary>
/// Agregado do Cliente. Detém o limite de crédito disponível para criação de Orders.
/// </summary>
public sealed class Customer
{
    public Guid Id { get; }

    public decimal CreditLimit { get; }

    public Customer(Guid id, decimal creditLimit)
    {
        if (creditLimit < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(creditLimit), "O limite de crédito do Cliente não pode ser negativo.");
        }

        Id = id;
        CreditLimit = creditLimit;
    }
}
