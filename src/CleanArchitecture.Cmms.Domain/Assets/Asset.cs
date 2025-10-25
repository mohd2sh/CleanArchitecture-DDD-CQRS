using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Assets.Entities;
using CleanArchitecture.Cmms.Domain.Assets.Enums;
using CleanArchitecture.Cmms.Domain.Assets.Events;
using CleanArchitecture.Cmms.Domain.Assets.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.Assets
{
    internal sealed class Asset : AggregateRoot<Guid>
    {
        private readonly List<MaintenanceRecord> _maintenanceRecords = new();
        public string Name { get; private set; }
        public string Type { get; private set; }
        public AssetTag Tag { get; private set; }
        public AssetLocation Location { get; private set; }
        public AssetStatus Status { get; private set; }
        public IReadOnlyCollection<MaintenanceRecord> MaintenanceRecords => _maintenanceRecords.AsReadOnly();
        private Asset() { } // EF

        private Asset(Guid id, string name, string type, AssetTag tag, AssetLocation location)
            : base(id)
        {
            Name = name;
            Type = type;
            Tag = tag;
            Location = location;
            Status = AssetStatus.Active;
        }

        public static Asset Create(string name, string type, AssetTag tag, AssetLocation location)
        {
            var asset = new Asset(Guid.NewGuid(), name, type, tag, location);

            asset.Raise(new AssetCreatedEvent(asset.Id, name, type, tag.Value));

            return asset;
        }

        public bool IsAvailable()
        {
            return Status == AssetStatus.Active;
        }

        public void ChangeStatus(AssetStatus newStatus)
        {
            if (Status == newStatus)
                return;

            Status = newStatus;
            Raise(new AssetStatusChangedEvent(Id, newStatus));
        }

        public void UpdateLocation(AssetLocation newLocation)
        {
            if (Location.Equals(newLocation))
                return;

            Location = newLocation;
            Raise(new AssetLocationUpdatedEvent(Id));
        }

        public void SetUnderMaintenance(string description, string performedBy, DateTime startedOn)
        {
            if (Status == AssetStatus.UnderMaintenance)
                throw new DomainException(AssetErrors.AlreadyUnderMaintenance);

            Status = AssetStatus.UnderMaintenance;

            var record = MaintenanceRecord.Create(Id, startedOn, description, performedBy);
            _maintenanceRecords.Add(record);

            Raise(new AssetStatusChangedEvent(Id, Status));
            Raise(new AssetMaintenanceStartedEvent(Id, record.Id, startedOn, description, performedBy));
        }

        public void CompleteMaintenance(DateTime completedOn, string notes)
        {
            if (Status != AssetStatus.UnderMaintenance)
                throw new DomainException(AssetErrors.NotUnderMaintenance);

            Status = AssetStatus.Active;
            Raise(new AssetStatusChangedEvent(Id, Status));
            Raise(new AssetMaintenanceCompletedEvent(Id, completedOn, notes));
        }

    }
}
