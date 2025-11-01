namespace CleanArchitecture.Cmms.Api.Controllers.V1.Requests.Technicans;

public sealed class ChangeTechnicianStatusRequest
{
    public int NewStatus { get; init; }
}
