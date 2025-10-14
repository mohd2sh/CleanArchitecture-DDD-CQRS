namespace CleanArchitecture.Cmms.Api.Controllers.V1.Requests.WorkOrders
{
    public sealed record CreateWorkOrderRequest(Guid AssetId, string Title, string Site, string Area, string Zone);
}
