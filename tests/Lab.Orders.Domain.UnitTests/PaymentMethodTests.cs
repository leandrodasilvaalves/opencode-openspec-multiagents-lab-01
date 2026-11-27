using FluentAssertions;
using Lab.Orders.Domain.Orders;
using Lab.Orders.Domain.Exceptions;
using Xunit;

namespace Lab.Orders.Domain.UnitTests;

public class PaymentMethodTests
{
    [Theory]
    [InlineData(PaymentMethod.Cash)]
    [InlineData(PaymentMethod.Pix)]
    [InlineData(PaymentMethod.CreditCard)]
    [InlineData(PaymentMethod.DebitCard)]
    public void Valid_payment_methods_are_accepted(PaymentMethod method)
    {
        Enum.IsDefined(typeof(PaymentMethod), method).Should().BeTrue();
    }

    [Fact]
    public void Unknown_payment_method_is_not_defined()
    {
        Enum.IsDefined(typeof(PaymentMethod), (PaymentMethod)99).Should().BeFalse();
    }

    [Fact]
    public void All_four_methods_exist()
    {
        Enum.GetValues<PaymentMethod>().Should().HaveCount(4);
        Enum.GetValues<PaymentMethod>().Should().Contain(PaymentMethod.Cash);
        Enum.GetValues<PaymentMethod>().Should().Contain(PaymentMethod.Pix);
        Enum.GetValues<PaymentMethod>().Should().Contain(PaymentMethod.CreditCard);
        Enum.GetValues<PaymentMethod>().Should().Contain(PaymentMethod.DebitCard);
    }
}
