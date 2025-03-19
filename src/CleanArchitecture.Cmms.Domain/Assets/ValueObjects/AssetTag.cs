using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Assets.ValueObjects
{
    public sealed record AssetTag(string Value) : ValueObject
    {
        public static AssetTag Create(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new DomainException("Asset tag cannot be empty.");

            return new(tag.Trim());
        }

        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
