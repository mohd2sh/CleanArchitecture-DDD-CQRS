using System.Net;
using CleanArchitecture.Cmms.Api.Controllers.V1.Requests.Technicans;
using CleanArchitecture.Cmms.Application.Technicians.Dtos;
using CleanArchitecture.Cmms.Domain.Technicians.Enums;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using CleanArchitecture.Cmms.IntegrationTests.Infrastructure;
using CleanArchitecture.Cmms.IntegrationTests.TestHelpers;

namespace CleanArchitecture.Cmms.IntegrationTests.Api.V1;

/// <summary>
/// Integration tests for Technician API endpoints
/// Tests HTTP endpoints with real database and full middleware pipeline
/// </summary>
public class TechniciansControllerTests : IntegrationTestBase
{
    public TechniciansControllerTests(CmmsWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task POST_CreateTechnician_Returns200_AndTechnicianId()
    {
        // Arrange
        var request = new CreateTechnicianRequest
        {
            Name = "John Doe",
            SkillLevelName = "Senior",
            SkillLevelRank = 3
        };

        // Act
        var response = await Client.PostAsJsonAsync(ApiEndpoints.Technicians.Create(), request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<Guid>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        // Verify in database
        var technician = await WriteDbContext.Technicians.FindAsync(result.Value);
        Assert.NotNull(technician);
        Assert.Equal("John Doe", technician.Name);
        Assert.Equal("Senior", technician.SkillLevel.LevelName);
        Assert.Equal(3, technician.SkillLevel.Rank);
        Assert.Equal(TechnicianStatus.Available, technician.Status);
    }

    [Fact]
    public async Task POST_CreateTechnician_WithInvalidData_Returns400()
    {
        // Arrange
        var request = new CreateTechnicianRequest
        {
            Name = "",
            SkillLevelRank = 3
        };

        // Act
        var response = await Client.PostAsJsonAsync(ApiEndpoints.Technicians.Create(), request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto>(response.Content);
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("name", result.Error.Message.ToLower());
    }

    [Fact]
    public async Task GET_TechnicianById_ReturnsCorrectTechnician()
    {
        // Arrange
        var technicianId = await CreateTechnicianAsync("Jane Smith", "Expert", 5);

        // Act
        var response = await Client.GetAsync(ApiEndpoints.Technicians.GetById(technicianId));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<TechnicianDto>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(technicianId, result.Value.Id);
        Assert.Equal("Jane Smith", result.Value.Name);
        Assert.Equal("Expert", result.Value.SkillLevelName);
        Assert.Equal(5, result.Value.SkillLevelRank);
        Assert.Equal("Available", result.Value.Status);
    }

    [Fact]
    public async Task GET_TechnicianById_WithNonExistentId_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync(ApiEndpoints.Technicians.GetById(nonExistentId));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<TechnicianDto>>(response.Content);
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("not found", result.Error.Message.ToLower());
    }

    [Fact]
    public async Task POST_AddCertification_UpdatesTechnician()
    {
        // Arrange
        var technicianId = await CreateTechnicianAsync("Certification Test Tech", "Senior", 3);
        var request = new AddCertificationRequest
        {
            Code = "WW-SOL-001",
            IssuedOn = DateTime.UtcNow.AddDays(-30),
            ExpiresOn = DateTime.UtcNow.AddDays(365)
        };

        // Act
        var response = await Client.PostAsJsonAsync(ApiEndpoints.Technicians.AddCertification(technicianId), request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        // Verify in database
        WriteDbContext.ChangeTracker.Clear();
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.NotNull(technician);
        Assert.Single(technician.Certifications);

        var certification = technician.Certifications.First();
        Assert.Equal("WW-SOL-001", certification.Code);
        Assert.Equal(request.IssuedOn.Date, certification.IssuedOn.Date);
        Assert.Equal(request.ExpiresOn.Value.Date, certification.ExpiresOn.Value.Date);
    }

    [Fact]
    public async Task POST_SetUnavailable_ChangesTechnicianAvailability()
    {
        // Arrange
        var technicianId = await CreateTechnicianAsync("Availability Test Tech", "Senior", 3);

        // Verify initial status
        var initialTechnician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.Equal(TechnicianStatus.Available, initialTechnician.Status);

        // Act
        var response = await Client.PostAsync(ApiEndpoints.Technicians.SetUnavailable(technicianId), null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        WriteDbContext.ChangeTracker.Clear();
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.NotNull(technician);
        Assert.Equal(TechnicianStatus.Unavailable, technician.Status);
    }

    [Fact]
    public async Task POST_SetAvailable_AllowsNewAssignments()
    {
        // Arrange
        var technicianId = await CreateTechnicianAsync("Available Test Tech", "Senior", 3);

        // Set technician as unavailable first
        await Client.PostAsync(ApiEndpoints.Technicians.SetUnavailable(technicianId), null);
        WriteDbContext.ChangeTracker.Clear();
        var unavailableTechnician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.Equal(TechnicianStatus.Unavailable, unavailableTechnician.Status);

        // Act
        var response = await Client.PostAsync(ApiEndpoints.Technicians.SetAvailable(technicianId), null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        // Verify in database
        WriteDbContext.ChangeTracker.Clear();
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.NotNull(technician);
        Assert.Equal(TechnicianStatus.Available, technician.Status);
    }

    [Fact]
    public async Task GET_AvailableTechnicians_ReturnsOnlyAvailable()
    {
        // Arrange
        await CreateTechnicianAsync("Available Tech 1", "Senior", 3);
        await CreateTechnicianAsync("Available Tech 2", "Expert", 5);
        var technician3Id = await CreateTechnicianAsync("Unavailable Tech", "Junior", 1);

        // Set one technician as unavailable
        await Client.PostAsync(ApiEndpoints.Technicians.SetUnavailable(technician3Id), null);

        // Act
        var response = await Client.GetAsync(ApiEndpoints.Technicians.GetAvailable(1, 10));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<PaginatedListDto<TechnicianDto>>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(2, result.Value.TotalCount);

        // Verify only available technicians are returned
        var technicianNames = result.Value.Items.Select(t => t.Name).ToList();
        Assert.Contains("Available Tech 1", technicianNames);
        Assert.Contains("Available Tech 2", technicianNames);
        Assert.DoesNotContain("Unavailable Tech", technicianNames);

        // Verify all returned technicians have Available status
        Assert.All(result.Value.Items, t => Assert.Equal(TechnicianStatus.Available.ToString(), t.Status));
    }

    [Fact]
    public async Task GET_AvailableTechnicians_WithPagination_ReturnsCorrectPage()
    {
        // Arrange - Create multiple technicians
        for (var i = 1; i <= 5; i++)
        {
            await CreateTechnicianAsync($"Tech {i}", "Senior", 3);
        }

        // Act - Get first page with page size 2
        var response = await Client.GetAsync(ApiEndpoints.Technicians.GetAvailable(1, 2));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<PaginatedListDto<TechnicianDto>>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(2, result.Value.PageSize);
        Assert.Equal(3, (int)Math.Ceiling((double)result.Value.TotalCount / result.Value.PageSize));
    }

    [Fact]
    public async Task POST_CreateTechnician_WithDuplicateName_ShouldSucceed()
    {
        // Arrange
        var request1 = new CreateTechnicianRequest
        {
            Name = "Duplicate Name",
            SkillLevelName = "Senior",
            SkillLevelRank = 3
        };

        var request2 = new CreateTechnicianRequest
        {
            Name = "Duplicate Name", // Same name
            SkillLevelName = "Expert",
            SkillLevelRank = 5
        };

        // Act
        var response1 = await Client.PostAsJsonAsync(ApiEndpoints.Technicians.Create(), request1);
        var response2 = await Client.PostAsJsonAsync(ApiEndpoints.Technicians.Create(), request2);

        // Assert - Both should succeed (names can be duplicated)
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var result1 = await JsonUtility.DeserializeAsync<ResultDto<Guid>>(response1.Content);
        var result2 = await JsonUtility.DeserializeAsync<ResultDto<Guid>>(response2.Content);

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(result1.Value, result2.Value); // Different IDs

        // Verify both technicians exist in database
        var technician1 = await WriteDbContext.Technicians.FindAsync(result1.Value);
        var technician2 = await WriteDbContext.Technicians.FindAsync(result2.Value);

        Assert.NotNull(technician1);
        Assert.NotNull(technician2);
        Assert.Equal("Duplicate Name", technician1.Name);
        Assert.Equal("Duplicate Name", technician2.Name);
    }

    [Fact]
    public async Task API_ShouldReturn_ConsistentErrorFormat()
    {
        // Arrange
        var invalidRequest = new CreateTechnicianRequest
        {
            Name = "",
            SkillLevelName = "",//Invalid
            SkillLevelRank = 3
        };

        // Act
        var response = await Client.PostAsJsonAsync(ApiEndpoints.Technicians.Create(), invalidRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto>(response.Content);
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Code);
        Assert.NotNull(result.Error.Message);
        Assert.NotEmpty(result.Error.Code);
        Assert.NotEmpty(result.Error.Message);
    }
}
