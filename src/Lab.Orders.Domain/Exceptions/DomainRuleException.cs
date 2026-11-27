using System.Diagnostics.CodeAnalysis;
using Lab.Orders.Domain.Contracts;

namespace Lab.Orders.Domain.Exceptions;

public class DomainRuleException : DomainException
{
    public IEnumerable<Error> Errors { get; }

    public DomainRuleException(IEnumerable<Error> errors) => Errors = errors;

    public static void ThrowIfAny(IEnumerable<Error> errors)
    {
        if (errors.Any())
        {
            Throw(errors);
        }
    }

    [DoesNotReturn]
    internal static void Throw(IEnumerable<Error> errors) =>
            throw new DomainRuleException(errors);
}
