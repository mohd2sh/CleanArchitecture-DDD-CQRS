using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Assets.Entities
{
    internal sealed class MaintenanceRecord : Entity<Guid>
    {
        public Guid AssetId { get; private set; }
        public DateTime StartedOn { get; private set; }
        public string Description { get; private set; }
        public string PerformedBy { get; private set; }

        private MaintenanceRecord() { } // For EF

        private MaintenanceRecord(Guid assetId, DateTime startedOn, string description, string performedBy)
            : base(Guid.NewGuid())
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException(AssetErrors.MaintenanceDescriptionRequired);

            if (string.IsNullOrWhiteSpace(performedBy))
                throw new DomainException(AssetErrors.MaintenancePerformerRequired);

            AssetId = assetId;
            StartedOn = startedOn;
            Description = description.Trim();
            PerformedBy = performedBy.Trim();
        }

        public static MaintenanceRecord Create(Guid assetId, DateTime startedOn, string description, string performedBy)
            => new(assetId, startedOn, description, performedBy);
    }
}
