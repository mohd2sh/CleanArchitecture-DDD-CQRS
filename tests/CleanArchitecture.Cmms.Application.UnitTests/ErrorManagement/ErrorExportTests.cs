using CleanArchitecture.Cmms.Application.ErrorManagement;

namespace CleanArchitecture.Cmms.Application.UnitTests.ErrorManagement;

public class ErrorExportTests
{
    private readonly ErrorExporter _sut;

    public ErrorExportTests()
    {
        _sut = new ErrorExporter();
    }

    [Fact]
    public void ExportDomainErrors_ShouldReturnDomainErrors()
    {
        // Act
        var domainErrors = _sut.ExportDomainErrors();

        // Assert
        domainErrors.Should().NotBeEmpty("Domain errors should be exported");
    }

    [Fact]
    public void ExportApplicationErrors_ShouldReturnApplicationErrors()
    {
        // Act
        var applicationErrors = _sut.ExportApplicationErrors();

        // Assert
        applicationErrors.Should().NotBeEmpty("Application errors should be exported");

        // Verify specific application errors exist
        applicationErrors.Should().ContainKey("WorkOrder.NotFound");
        applicationErrors.Should().ContainKey("Technician.NotFound");
        applicationErrors.Should().ContainKey("Asset.NotFound");
    }

    [Fact]
    public void ExportAll_ShouldReturnBothDomainAndApplicationErrors()
    {
        // Act
        var result = _sut.ExportAll();

        // Assert
        result.DomainErrors.Should().NotBeEmpty("Domain errors should be exported");
        result.ApplicationErrors.Should().NotBeEmpty("Application errors should be exported");
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }
}
