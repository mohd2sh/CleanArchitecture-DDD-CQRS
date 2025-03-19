using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Technicians.ValueObjects
{
    public sealed record Certification(string Code, DateTime IssuedOn, DateTime? ExpiresOn) : ValueObject
    {
        public bool IsValid(DateTime nowUtc)
            => ExpiresOn is null || ExpiresOn > nowUtc;

        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return Code;
            yield return IssuedOn;
            yield return ExpiresOn;
        }

        public override string ToString() => $"{Code} (Issued: {IssuedOn:yyyy-MM-dd})";
    }
}
