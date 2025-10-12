namespace CleanArchitecture.Cmms.Api.Controllers.V1.Requests.Technicans
{
    public sealed class AddCertificationRequest
    {
        public string Code { get; init; } = default!;
        public DateTime IssuedOn { get; init; }
        public DateTime? ExpiresOn { get; init; }
    }

}
