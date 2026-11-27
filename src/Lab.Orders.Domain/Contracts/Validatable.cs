using Lab.Orders.Domain.Exceptions;

namespace Lab.Orders.Domain.Contracts;

public abstract class Validatable
{
    private readonly List<Error> _errors = [];

    public IReadOnlyCollection<Error> Errors => _errors.AsReadOnly();

    protected void AddError(string message)
    {
        var error = new Error(ErrorCode(), message);
        if (!_errors.Contains(error))
        {
            _errors.Add(error);
        }
    }

    protected void ClearErrors() => _errors.Clear();

    protected bool IsValid() => _errors.Count == 0;

    protected bool HasError(Error error) => _errors.Contains(error);

    protected abstract void Validate();

    protected abstract string ErrorCode();

    public void ValidateThrow()
    {
        ClearErrors();
        Validate();
        if (!IsValid())
        {
            throw new DomainRuleException(Errors);
        }
    }
}
