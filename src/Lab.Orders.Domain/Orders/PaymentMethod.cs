using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Domain.Orders;

public enum PaymentMethod
{
    Cash,
    Pix,
    CreditCard,
    DebitCard
}

public static class PaymentMethodParser
{
    public static bool TryParse(string? value, out PaymentMethod paymentMethod)
    {
        paymentMethod = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();

        if (Enum.TryParse<PaymentMethod>(trimmed, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed))
        {
            paymentMethod = parsed;
            return true;
        }

        return false;
    }

    public static PaymentMethod Parse(string? value)
    {
        if (!TryParse(value, out var paymentMethod))
            throw new DomainException($"Invalid payment method. Accepted values: {string.Join(", ", Enum.GetNames<PaymentMethod>())}.");

        return paymentMethod;
    }

    public static bool IsValid(PaymentMethod paymentMethod) => Enum.IsDefined(paymentMethod);
}
