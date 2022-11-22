namespace RevolutionaryStuff.Core;

public class Missingable<T>
{
    public static readonly Missingable<T> Missing = new();

    private Missingable()
    {
        IsReadonly = true;
    }

    public Missingable(T value)
    {
        Value = value;
    }

    private readonly bool IsReadonly;

    public bool HasValue { get; private set; }

    public void ClearValue()
    {
        Requires.False(IsReadonly);
        HasValue = false;
    }

    public T Value
    {
        get
        {
            return !HasValue ? throw new InvalidOperationException() : Value_p;
        }
        set
        {
            Requires.False(IsReadonly);
            Value_p = value;
            HasValue = true;
        }
    }
    private T Value_p;

    public override bool Equals(object other)
    {
        if (other is not Missingable<T> that) return false;
        return !HasValue && !that.HasValue || HasValue && Value.Equals(that.Value);
    }

    public override int GetHashCode()
    {
        return !HasValue ? 0 : Value.GetHashCode();
    }

    public static explicit operator T(Missingable<T> value)
    {
        return value.Value;
    }

    public static implicit operator Missingable<T>(T value)
    {
        return new Missingable<T>(value);
    }

    public override string ToString() => HasValue ? $"{GetType().Name} value={Value}" : $"{GetType().Name} isMissing=true";
}
