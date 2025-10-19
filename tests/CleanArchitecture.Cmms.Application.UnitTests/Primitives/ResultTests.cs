using CleanArchitecture.Cmms.Application.Primitives;

namespace CleanArchitecture.Cmms.Application.UnitTests.Primitives;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var error = Error.Failure("Test.Failure", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        // Arrange
        var error = Error.Failure("Test.Failure", "Test error message");

        // Act
        Result result = error;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("Test.Failure");
        result.Error.Message.Should().Be("Test error message");
        result.Error.Type.Should().Be(ErrorType.Failure);
    }
}

public class ResultTTests
{
    [Fact]
    public void Success_WithValue_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var value = "Test value";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var error = Error.Failure("Test.Failure", "Test error message");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccess()
    {
        // Arrange
        var value = 123;

        // Act
        Result<int> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        // Arrange
        var error = Error.Failure("Test.Failure", "Test error message");

        // Act
        Result<string> result = error;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("Test.Failure");
        result.Error.Message.Should().Be("Test error message");
        result.Error.Type.Should().Be(ErrorType.Failure);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(null)]
    public void ImplicitConversion_FromValue_ShouldHandleVariousValues(int? value)
    {
        // Act
        Result<int?> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Error.Should().BeNull();
    }
}
