using Lab.Orders.Domain.Exceptions;

namespace Lab.Orders.Domain.Contracts;

public abstract class Validatable
{
    private readonly List<Error> _errors = [];

    public IReadOnlyCollection<Error> Errors => _errors;

    protected void AddError(string message)
    {
        var error = new Error(ErrorCode(), message);
        if (error is not null && !HasError(error))
        {
            _errors.Add(error);
        }
    }

    protected void AddErrors(IEnumerable<Error> errors)
    {
        DomainNullException.ThrowIfNull(errors);
        _errors.AddRange(errors);
    }

    protected void ClearErrors() => _errors.Clear();

    protected bool IsValid() => _errors.Count == 0;

    protected bool IsInvalid() => !IsValid();

    protected bool HasError(Error error) => _errors.Contains(error);

    protected abstract void Validate();

    protected abstract string ErrorCode();

    protected void Validate(Validatable? validateble)
    {
        DomainNullException.ThrowIfNull(validateble);

        validateble.Validate();
        if (validateble.IsInvalid())
        {
            var sufixError = validateble.GetType().Name.ToUpper();
            AddErrors(validateble.Errors.Select(e =>
                new Error($"{ErrorCode}_{sufixError}", e.Message)));
        }
    }
}