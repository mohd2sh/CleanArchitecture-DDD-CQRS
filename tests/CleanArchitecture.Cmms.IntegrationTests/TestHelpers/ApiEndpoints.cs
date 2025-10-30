namespace CleanArchitecture.Cmms.IntegrationTests.TestHelpers;

public static class ApiEndpoints
{
    private const string BaseV1 = "/api/v1";

    public static class Technicians
    {
        private const string Base = $"{BaseV1}/technicians";

        public static string Create() => Base;
        public static string GetById(Guid id) => $"{Base}/{id}";
        public static string GetAvailable(int pageNumber = 1, int pageSize = 20)
            => $"{Base}/available?pageNumber={pageNumber}&pageSize={pageSize}";
        public static string AddCertification(Guid id) => $"{Base}/{id}/certifications";
        public static string SetAvailable(Guid id) => $"{Base}/{id}/set-available";
        public static string SetUnavailable(Guid id) => $"{Base}/{id}/set-unavailable";
    }

    public static class Assets
    {
        private const string Base = $"{BaseV1}/assets";

        public static string Create() => Base;
        public static string GetById(Guid id) => $"{Base}/{id}";
        public static string GetActive(int pageNumber = 1, int pageSize = 20)
            => $"{Base}/active?pageNumber={pageNumber}&pageSize={pageSize}";
    }

    public static class WorkOrders
    {
        private const string Base = $"{BaseV1}/workorders";

        public static string Create() => Base;
        public static string GetById(Guid id) => $"{Base}/{id}";
        public static string Start(Guid id) => $"{Base}/{id}/start";
        public static string Complete(Guid id) => $"{Base}/{id}/complete";
        public static string AddStep(Guid id) => $"{Base}/{id}/steps";
    }
}

