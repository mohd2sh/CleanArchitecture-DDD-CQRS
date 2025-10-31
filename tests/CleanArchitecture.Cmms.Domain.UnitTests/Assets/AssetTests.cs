using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.Assets.Enums;
using CleanArchitecture.Cmms.Domain.Assets.ValueObjects;
using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Assets;

public class AssetTests
{
    [Fact]
    public void Create_Should_Initialize_Properties_And_Default_Status_Active()
    {
        // Arrange
        var assetTag = AssetTag.Create("A-100");
        var assetLocation = AssetLocation.Create("Site1", "AreaA", "ZoneZ");
        var assetName = "Pump";
        var assetType = "Mechanical";

        // Act
        var asset = Asset.Create(assetName, assetType, assetTag, assetLocation);

        // Assert
        Assert.Equal(assetName, asset.Name);
        Assert.Equal(assetType, asset.Type);
        Assert.Equal(assetTag, asset.Tag);
        Assert.Equal(assetLocation, asset.Location);
        Assert.Equal(AssetStatus.Active, asset.Status);
        Assert.Empty(asset.MaintenanceRecords);
    }

    [Fact]
    public void IsAvailable_Should_Return_True_When_Status_Active()
    {
        // Arrange
        var asset = Asset.Create("Compressor", "Mechanical", AssetTag.Create("C-100"), AssetLocation.Create("S1", "A", "Z"));

        // Act
        var isAvailable = asset.IsAvailable();

        // Assert
        Assert.True(isAvailable);
    }

    [Fact]
    public void ChangeStatus_Should_Update_Status_When_Different()
    {
        // Arrange
        var asset = Asset.Create("Valve", "Mechanical", AssetTag.Create("V-1"), AssetLocation.Create("S1", "A", "Z"));
        var newStatus = AssetStatus.Inactive;

        // Act
        asset.ChangeStatus(newStatus);

        // Assert
        Assert.Equal(newStatus, asset.Status);
    }

    [Fact]
    public void ChangeStatus_Should_Do_Nothing_When_Same_Status()
    {
        // Arrange
        var asset = Asset.Create("Valve", "Mechanical", AssetTag.Create("V-1"), AssetLocation.Create("S1", "A", "Z"));
        var sameStatus = asset.Status;

        // Act
        asset.ChangeStatus(sameStatus);

        // Assert
        Assert.Equal(AssetStatus.Active, asset.Status);
    }

    [Fact]
    public void UpdateLocation_Should_Set_New_Location_When_Different()
    {
        // Arrange
        var asset = Asset.Create("Motor", "Electrical", AssetTag.Create("M-100"), AssetLocation.Create("S1", "A", "Z"));
        var newLocation = AssetLocation.Create("S1", "B", "Z");

        // Act
        asset.UpdateLocation(newLocation);

        // Assert
        Assert.Equal(newLocation, asset.Location);
    }

    [Fact]
    public void UpdateLocation_Should_Do_Nothing_When_Same_Location()
    {
        // Arrange
        var originalLocation = AssetLocation.Create("S1", "A", "Z");
        var asset = Asset.Create("Generator", "Electrical", AssetTag.Create("G-10"), originalLocation);

        // Act
        asset.UpdateLocation(originalLocation);

        // Assert
        Assert.Equal(originalLocation, asset.Location);
    }

    [Fact]
    public void SetUnderMaintenance_Should_Set_Status_Add_Record_And_Raise_Events()
    {
        // Arrange
        var asset = Asset.Create("Pump", "Mechanical", AssetTag.Create("P-1"), AssetLocation.Create("S1", "A", "Z"));
        var description = "Oil Change";
        var performedBy = "Technician A";
        var startedOn = DateTime.UtcNow;

        // Act
        asset.SetUnderMaintenance(description, performedBy, startedOn);

        // Assert
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);
        Assert.Single(asset.MaintenanceRecords);
        var record = asset.MaintenanceRecords.First();
        Assert.Equal(description, record.Description);
        Assert.Equal(performedBy, record.PerformedBy);
        Assert.Equal(asset.Id, record.AssetId);
        Assert.Equal(startedOn, record.StartedOn);
    }

    [Fact]
    public void SetUnderMaintenance_Should_Throw_When_Already_UnderMaintenance()
    {
        // Arrange
        var asset = Asset.Create("Pump", "Mechanical", AssetTag.Create("P-1"), AssetLocation.Create("S1", "A", "Z"));
        var firstStartedOn = DateTime.UtcNow.AddMinutes(-5);
        asset.SetUnderMaintenance("Fix", "Tech", firstStartedOn);

        // Act
        void act() => asset.SetUnderMaintenance("Fix again", "Tech", DateTime.UtcNow);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void CompleteMaintenance_Should_Set_Status_To_Active_When_Previously_UnderMaintenance()
    {
        // Arrange
        var asset = Asset.Create("Pump", "Mechanical", AssetTag.Create("P-1"), AssetLocation.Create("S1", "A", "Z"));
        var startedOn = DateTime.UtcNow.AddMinutes(-10);
        asset.SetUnderMaintenance("Inspect", "Tech", startedOn);
        var completedOn = DateTime.UtcNow;
        var notes = "Completed";

        // Act
        asset.CompleteMaintenance(completedOn, notes);

        // Assert
        Assert.Equal(AssetStatus.Active, asset.Status);
    }

    [Fact]
    public void CompleteMaintenance_Should_Throw_When_Not_UnderMaintenance()
    {
        // Arrange
        var asset = Asset.Create("Pump", "Mechanical", AssetTag.Create("P-1"), AssetLocation.Create("S1", "A", "Z"));
        var completedOn = DateTime.UtcNow;

        // Act
        void act() => asset.CompleteMaintenance(completedOn, "Notes");

        // Assert
        Assert.Throws<DomainException>(act);
    }
}
