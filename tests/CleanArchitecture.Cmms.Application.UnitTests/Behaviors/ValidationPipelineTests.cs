using CleanArchitecture.Cmms.Application.Behaviors;
using FluentValidation;

namespace CleanArchitecture.Cmms.Application.UnitTests.Behaviors;

public class ValidationPipelineTests
{
    private sealed class DummyRequest { public int X { get; init; } }

    private sealed class DummyValidator : AbstractValidator<DummyRequest>
    {
        public DummyValidator(bool valid)
        {
            if (!valid)
            {
                RuleFor(x => x.X).GreaterThan(0);
            }
        }
    }

    [Fact]
    public async Task Should_Throw_When_Validation_Fails()
    {
        // Arrange
        var validators = new List<IValidator<DummyRequest>> { new DummyValidator(valid: false) };
        var pipe = new ValidationPipeline<DummyRequest, int>(validators);

        // Act
        Func<Task> act = async () => await pipe.Handle(new DummyRequest { X = 0 }, () => Task.FromResult(1), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task Should_Continue_When_Validation_Passes()
    {
        // Arrange
        var validators = new List<IValidator<DummyRequest>> { new DummyValidator(valid: true) };
        var pipe = new ValidationPipeline<DummyRequest, int>(validators);
        var expected = 42;

        // Act
        var result = await pipe.Handle(new DummyRequest { X = 0 }, () => Task.FromResult(expected), CancellationToken.None);

        // Assert
        Assert.Equal(expected, result);
    }
}
