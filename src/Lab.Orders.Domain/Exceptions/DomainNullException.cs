using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Lab.Orders.Domain.Exceptions;

public class DomainNullException : Exception
{
    public DomainNullException(string? message) : base(message)
    { }

    public static void ThrowIfNull([NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            Throw(paramName);
        }
    }

    [DoesNotReturn]
    internal static void Throw(string? paramName) =>
            throw new DomainNullException(paramName);
}
