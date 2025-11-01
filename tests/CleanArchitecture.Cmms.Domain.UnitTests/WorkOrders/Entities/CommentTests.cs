using CleanArchitecture.Cmms.Domain.WorkOrders.Entities;

namespace CleanArchitecture.Cmms.Domain.UnitTests.WorkOrders.Entities;

public class CommentTests
{
    [Fact]
    public void Create_Should_Set_Text_And_AuthorId()
    {
        // Arrange
        var text = "Note 1";
        var authorId = Guid.NewGuid();

        // Act
        var comment = Comment.Create(text, authorId);

        // Assert
        Assert.Equal(text, comment.Text);
        Assert.Equal(authorId, comment.AuthorId);
    }
}
