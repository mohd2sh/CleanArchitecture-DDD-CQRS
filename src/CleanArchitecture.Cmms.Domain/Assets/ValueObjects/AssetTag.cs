using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Assets.ValueObjects
{
    internal sealed record AssetTag : ValueObject
    {
        public string Value { get; }

        private AssetTag() { } //For Ef

        private AssetTag(string value)
        {
            Value = value;
        }

        public static AssetTag Create(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new DomainException(AssetErrors.TagRequired);

            return new(tag.Trim());
        }

        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
