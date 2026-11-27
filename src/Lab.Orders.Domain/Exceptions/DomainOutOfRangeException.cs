using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Lab.Orders.Domain.Exceptions;

public class DomainOutOfRangeException : DomainException
{
    public DomainOutOfRangeException(string? message) : base(message) { }

    public static void ThrowIfLessThan<T>(T value, T other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(other) < 0)
            Throw(value, other, paramName);
    }

    [DoesNotReturn]
    private static void Throw<T>(T value, T other, string? paramName) =>
        throw new DomainOutOfRangeException($"{paramName} must be greater than or equal to {other}.");
}