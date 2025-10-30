using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteStep;
using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.UnitTests.WorkOrders.Commands.CompleteStep;

public class CompleteStepCommandHandlerTests
{
    private readonly Mock<IRepository<WorkOrder, Guid>> _repositoryMock;
    private readonly CompleteStepCommandHandler _sut;

    public CompleteStepCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<WorkOrder, Guid>>();
        _sut = new CompleteStepCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderExists_ShouldCompleteStep()
    {
        // Arrange
        var workOrder = CreateWorkOrderWithStep(out var stepId);
        var command = new CompleteStepCommand(workOrder.Id, stepId);

        _repositoryMock.Setup(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Verify(x => x.GetByIdAsync(workOrder.Id, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(workOrder, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var command = new CompleteStepCommand(workOrderId, stepId);

        _repositoryMock.Setup(x => x.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(Application.WorkOrders.WorkOrderErrors.NotFound.Code);
        result.Error.Message.Should().Be(Application.WorkOrders.WorkOrderErrors.NotFound.Message);

        _repositoryMock.Verify(x => x.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static WorkOrder CreateWorkOrderWithStep(out Guid stepId)
    {
        var location = Location.Create("Building A", "Floor 1", "Room 101");
        var workOrder = WorkOrder.Create(Guid.NewGuid(), "Test Work Order", location);
        workOrder.AddStep("Step 1");
        stepId = workOrder.Steps.First().Id;
        return workOrder;
    }
}


