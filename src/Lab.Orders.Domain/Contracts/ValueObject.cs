namespace Lab.Orders.Domain.Contracts;

public abstract class ValueObject<T> : Validateble, IEquatable<T>
{
    public abstract bool Equals(T? other);
    public abstract override bool Equals(object? obj);
    public abstract override int GetHashCode();
    public abstract override string ToString();
}
