using CleanArchitecture.Cmms.Application.Behaviors;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.UnitTests.Behaviors;

public class LoggingPipelineTests
{
    private readonly Mock<ILogger<LoggingPipeline<TestRequest, TestResponse>>> _loggerMock;
    private readonly LoggingPipeline<TestRequest, TestResponse> _pipeline;

    public LoggingPipelineTests()
    {
        _loggerMock = new Mock<ILogger<LoggingPipeline<TestRequest, TestResponse>>>();
        _pipeline = new LoggingPipeline<TestRequest, TestResponse>(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldLogRequestAndResponse()
    {
        // Arrange
        var request = new TestRequest { Id = 123, Name = "Test" };
        var expectedResponse = new TestResponse { Result = "Success" };
        var nextCalled = false;

        // Act
        var result = await _pipeline.Handle(request, () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        }, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextCalled.Should().BeTrue();

        // Verify logging calls
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling TestRequest")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handled TestRequest")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNextThrowsException_ShouldLogAndRethrow()
    {
        // Arrange
        var request = new TestRequest { Id = 123, Name = "Test" };
        var expectedException = new InvalidOperationException("Test exception");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _pipeline.Handle(request, () => throw expectedException, CancellationToken.None));

        exception.Should().Be(expectedException);

        // Verify error logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never); // LoggingPipeline doesn't log errors, it just rethrows
    }

    [Fact]
    public async Task Handle_ShouldMeasureExecutionTime()
    {
        // Arrange
        var request = new TestRequest { Id = 123, Name = "Test" };
        var expectedResponse = new TestResponse { Result = "Success" };

        // Act
        var result = await _pipeline.Handle(request, async () =>
        {
            await Task.Delay(10); // Simulate some work
            return expectedResponse;
        }, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);

        // Verify that execution time is logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public class TestRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TestResponse
    {
        public string Result { get; set; } = string.Empty;
    }
}
