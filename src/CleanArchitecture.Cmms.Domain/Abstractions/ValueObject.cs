namespace CleanArchitecture.Cmms.Domain.Abstractions
{
    public abstract record ValueObject
    {
        public virtual bool Equals(ValueObject? other)
        {
            if (other is null || other.GetType() != GetType())
                return false;

            var thisValues = GetAtomicValues().ToArray();
            var otherValues = other.GetAtomicValues().ToArray();

            if (thisValues.Length != otherValues.Length)
                return false;

            for (var i = 0; i < thisValues.Length; i++)
            {
                var a = thisValues[i];
                var b = otherValues[i];

                if (a is null ^ b is null)
                    return false;

                if (a is not null && !a.Equals(b))
                    return false;
            }

            return true;
        }

        public override int GetHashCode() =>
            GetAtomicValues()
                .Select(v => v?.GetHashCode() ?? 0)
                .Aggregate((x, y) => x ^ y);

        protected static bool EqualOperator(ValueObject? left, ValueObject? right)
        {
            if (left is null ^ right is null)
                return false;

            return left?.Equals(right) != false;
        }

        protected static bool NotEqualOperator(ValueObject? left, ValueObject? right)
            => !EqualOperator(left, right);

        protected abstract IEnumerable<object?> GetAtomicValues();
    }
}
