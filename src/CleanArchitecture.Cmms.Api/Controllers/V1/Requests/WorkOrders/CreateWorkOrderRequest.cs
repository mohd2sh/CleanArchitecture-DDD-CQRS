namespace CleanArchitecture.Cmms.Api.Controllers.V1.Requests.WorkOrders
{
    public sealed record CreateWorkOrderRequest(string Title, string Site, string Area, string Zone);
}
