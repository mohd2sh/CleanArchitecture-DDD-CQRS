using CleanArchitecture.Cmms.Application.Primitives;

namespace CleanArchitecture.Cmms.Application.UnitTests.Primitives;

public class ErrorExportTests
{
    [Fact]
    public void ExportDomainErrors_ShouldReturnDomainErrors()
    {
        // Act
        var domainErrors = ErrorExporter.ExportDomainErrors();

        // Assert
        domainErrors.Should().NotBeEmpty("Domain errors should be exported");

        // Verify specific domain errors exist
        domainErrors.Should().ContainKey("WorkOrder.TitleRequired");
        domainErrors.Should().ContainKey("WorkOrder.AssetIdRequired");
        domainErrors.Should().ContainKey("Technician.Unavailable");
        domainErrors.Should().ContainKey("Asset.AlreadyUnderMaintenance");

        // Verify error structure
        var titleRequired = domainErrors["WorkOrder.TitleRequired"];
        titleRequired.Code.Should().Be("WorkOrder.TitleRequired");
        titleRequired.Message.Should().Be("Work order title cannot be empty.");
        titleRequired.Domain.Should().Be("WorkOrder");
        titleRequired.ClassName.Should().Be("WorkOrderErrors");
        titleRequired.FieldName.Should().Be("TitleRequired");
    }

    [Fact]
    public void ExportApplicationErrors_ShouldReturnApplicationErrors()
    {
        // Act
        var applicationErrors = ErrorExporter.ExportApplicationErrors();

        // Assert
        applicationErrors.Should().NotBeEmpty("Application errors should be exported");

        // Verify specific application errors exist
        applicationErrors.Should().ContainKey("WorkOrder.NotFound");
        applicationErrors.Should().ContainKey("Technician.NotFound");
        applicationErrors.Should().ContainKey("Asset.NotFound");

        // Verify error structure
        var notFound = applicationErrors["WorkOrder.NotFound"];
        notFound.Code.Should().Be("WorkOrder.NotFound");
        notFound.Message.Should().Be("Work order not found.");
        notFound.Type.Should().Be("NotFound");
        notFound.Domain.Should().Be("WorkOrder");
        notFound.ClassName.Should().Be("WorkOrderErrors");
        notFound.FieldName.Should().Be("NotFound");
    }

    [Fact]
    public void ExportAll_ShouldReturnBothDomainAndApplicationErrors()
    {
        // Act
        var result = ErrorExporter.ExportAll();

        // Assert
        result.DomainErrors.Should().NotBeEmpty("Domain errors should be exported");
        result.ApplicationErrors.Should().NotBeEmpty("Application errors should be exported");
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        // Verify we have errors from all domains
        result.DomainErrors.Should().ContainKey("WorkOrder.TitleRequired");
        result.DomainErrors.Should().ContainKey("Technician.Unavailable");
        result.DomainErrors.Should().ContainKey("Asset.AlreadyUnderMaintenance");

        result.ApplicationErrors.Should().ContainKey("WorkOrder.NotFound");
        result.ApplicationErrors.Should().ContainKey("Technician.NotFound");
        result.ApplicationErrors.Should().ContainKey("Asset.NotFound");
    }
}
