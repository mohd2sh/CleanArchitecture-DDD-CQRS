using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;

internal sealed record Certification : ValueObject
{
    public string Code { get; private set; }
    public DateTime IssuedOn { get; private set; }
    public DateTime? ExpiresOn { get; private set; }

    private Certification(string code, DateTime issuedOn, DateTime? expiresOn)
    {
        Code = code;
        IssuedOn = issuedOn;
        ExpiresOn = expiresOn;
    }

    private Certification() { }

    public static Certification Create(string code, DateTime issuedOn, DateTime? expiresOn)
        => new(code.Trim(), issuedOn, expiresOn);

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
