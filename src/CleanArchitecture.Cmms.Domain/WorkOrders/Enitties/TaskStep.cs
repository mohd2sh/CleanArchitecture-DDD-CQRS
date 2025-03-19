using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.WorkOrders.Enitties
{
    internal sealed class TaskStep : Entity<Guid>
    {
        private TaskStep() { }
        internal TaskStep(string description)
        {
            Id = Guid.NewGuid();
            Description = description;
            Completed = false;
        }

        public string Description { get; private set; } = default!;
        public bool Completed { get; private set; }

        internal void MarkCompleted()
        {
            if (Completed) return;
            Completed = true;
        }
    }
}
