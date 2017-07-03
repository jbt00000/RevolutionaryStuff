using System;

namespace RevolutionaryStuff.Core
{
    public class Missingable<T>
    {
        public static readonly Missingable<T> Missing = new Missingable<T>();

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
            Requires.False(IsReadonly, nameof(IsReadonly));
            HasValue = false;
        }

        public T Value
        {
            get
            {
                if (!HasValue) throw new InvalidOperationException();
                return Value_p;
            }
            set
            {
                Requires.False(IsReadonly, nameof(IsReadonly));
                Value_p = value;
                HasValue = true;
            }
        }
        private T Value_p;

        public override bool Equals(object other)
        {
            var that = other as Missingable<T>;
            if (that == null) return false;
            if (!this.HasValue && !that.HasValue) return true;
            if (!this.HasValue) return false;
            return Value.Equals(that.Value);
        }

        public override int GetHashCode()
        {
            if (!this.HasValue) return 0;
            return Value.GetHashCode();
        }

        public static explicit operator T(Missingable<T> value)
        {
            return value.Value;
        }

        public static implicit operator Missingable<T>(T value)
        {
            return new Missingable<T>(value);
        }

        public override string ToString() => HasValue ? $"{this.GetType().Name} value={Value}" : $"{this.GetType().Name} isMissing=true";
    }
}
