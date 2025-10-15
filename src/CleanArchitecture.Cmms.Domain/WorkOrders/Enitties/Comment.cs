using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.WorkOrders.Enitties
{

    internal sealed class Comment : Entity<Guid>
    {
        private Comment() { }
        internal Comment(string text, Guid authorId)
        {
            Id = Guid.NewGuid();
            Text = text;
            AuthorId = authorId;
        }

        public string Text { get; private set; } = default!;
        public Guid AuthorId { get; private set; }
    }
}