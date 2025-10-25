using CleanArchitecture.Cmms.Application.Abstractions.Query;

namespace CleanArchitecture.Cmms.Application.UnitTests.Abstractions.Query;

public class PaginatedListTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreatePaginatedList()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };
        var totalCount = 10;
        var pageNumber = 1;
        var pageSize = 3;

        // Act
        var result = PaginatedList<string>.Create(items, totalCount, pageNumber, pageSize);

        // Assert
        result.Items.Should().BeEquivalentTo(items);
        result.TotalCount.Should().Be(totalCount);
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public void CreateFromOffset_WithSkipAndTake_WhenTakeIsNull_ShouldUseTotalCountAsPageSize()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2" };
        var totalCount = 10;
        var skip = 4;
        int? take = null;

        // Act
        var result = PaginatedList<string>.CreateFromOffset(items, totalCount, skip, take);

        // Assert
        result.Items.Should().BeEquivalentTo(items);
        result.TotalCount.Should().Be(totalCount);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(totalCount);
    }

    [Fact]
    public void CreateFromOffset_WithSkipAndTake_WhenSkipIsNull_ShouldUsePageNumberOne()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2" };
        var totalCount = 10;
        int? skip = null;
        var take = 2;

        // Act
        var result = PaginatedList<string>.CreateFromOffset(items, totalCount, skip, take);

        // Assert
        result.Items.Should().BeEquivalentTo(items);
        result.TotalCount.Should().Be(totalCount);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(take);
    }

    [Fact]
    public void ToNew_ShouldCreateNewPaginatedListWithDifferentItems()
    {
        // Arrange
        var originalItems = new List<string> { "Item1", "Item2" };
        var newItems = new List<int> { 1, 2, 3 };
        var totalCount = 10;
        var pageNumber = 1;
        var pageSize = 2;

        var originalList = PaginatedList<string>.Create(originalItems, totalCount, pageNumber, pageSize);

        // Act
        var result = originalList.ToNew(newItems);

        // Assert
        result.Items.Should().BeEquivalentTo(newItems);
        result.TotalCount.Should().Be(totalCount);
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public void Create_WithEmptyItems_ShouldWorkCorrectly()
    {
        // Arrange
        var items = new List<string>();
        var totalCount = 0;
        var pageNumber = 1;
        var pageSize = 10;

        // Act
        var result = PaginatedList<string>.Create(items, totalCount, pageNumber, pageSize);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Theory]
    [InlineData(0, 5, 1)]
    [InlineData(5, 5, 2)]
    [InlineData(10, 5, 3)]
    [InlineData(15, 5, 4)]
    public void CreateFromOffset_WithSkipAndTake_ShouldCalculatePageNumberCorrectly(int skip, int take, int expectedPageNumber)
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2" };
        var totalCount = 20;

        // Act
        var result = PaginatedList<string>.CreateFromOffset(items, totalCount, skip, take);

        // Assert
        result.PageNumber.Should().Be(expectedPageNumber);
        result.PageSize.Should().Be(take);
    }
}
