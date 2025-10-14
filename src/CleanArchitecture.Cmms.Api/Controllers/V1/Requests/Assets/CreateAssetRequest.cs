namespace CleanArchitecture.Cmms.Api.Controllers.V1.Requests.Assets
{
    public sealed class CreateAssetRequest
    {
        public string Name { get; init; } = default!;
        public string Type { get; init; } = default!;
        public string TagCode { get; init; } = default!;
        public string Site { get; init; } = default!;
        public string Area { get; init; } = default!;
        public string Zone { get; init; } = default!;
    }
}
