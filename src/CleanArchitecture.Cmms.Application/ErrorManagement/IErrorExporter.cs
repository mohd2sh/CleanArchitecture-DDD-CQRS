using CleanArchitecture.Cmms.Application.ErrorManagement.Models;

namespace CleanArchitecture.Cmms.Application.ErrorManagement
{
    public interface IErrorExporter
    {
        ErrorExportResult ExportAll();
        Dictionary<string, ApplicationErrorInfo> ExportApplicationErrors();
        Dictionary<string, DomainErrorInfo> ExportDomainErrors();
    }
}