using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.WorkOrders.Entities
{
    internal sealed class TaskStep : Entity<Guid>
    {
        private TaskStep() { }
        private TaskStep(string description)
        {
            Id = Guid.NewGuid();
            Description = description;
            Completed = false;
        }

        public static TaskStep Create(string description)
            => new(description);

        public string Description { get; private set; } = default!;
        public bool Completed { get; private set; }

        internal void MarkCompleted()
        {
            if (Completed) return;
            Completed = true;
        }
    }
}
