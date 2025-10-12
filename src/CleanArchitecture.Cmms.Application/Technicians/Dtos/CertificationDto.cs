namespace CleanArchitecture.Cmms.Application.Technicians.Dtos
{
    public sealed class CertificationDto
    {
        public string Code { get; init; } = default!;
        public DateTime IssuedOn { get; init; }
        public DateTime? ExpiresOn { get; init; }
        public bool IsValid { get; init; }
    }
}
