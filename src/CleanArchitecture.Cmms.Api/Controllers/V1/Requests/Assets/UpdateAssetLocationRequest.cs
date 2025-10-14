namespace CleanArchitecture.Cmms.Api.Controllers.V1.Requests.Assets
{
    public sealed class UpdateAssetLocationRequest
    {
        public string Site { get; init; } = default!;
        public string Area { get; init; } = default!;
        public string Zone { get; init; } = default!;
    }

}
