using CleanArchitecture.Cmms.Domain.WorkOrders.Enitties;

namespace CleanArchitecture.Cmms.Domain.UnitTests.WorkOrders.Entities
{
    public class CommentTests
    {
        [Fact]
        public void Create_Should_Set_Text_And_AuthorId()
        {
            // Arrange
            string text = "Note 1";
            Guid authorId = Guid.NewGuid();

            // Act
            Comment comment = Comment.Create(text, authorId);

            // Assert
            Assert.Equal(text, comment.Text);
            Assert.Equal(authorId, comment.AuthorId);
        }
    }
}
