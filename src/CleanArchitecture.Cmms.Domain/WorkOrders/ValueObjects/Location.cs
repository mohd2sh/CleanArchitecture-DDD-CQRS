using CleanArchitecture.Core.Domain.Abstractions  ;

namespace CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects
{
    internal sealed record Location : ValueObject
    {
        public string Building { get; }
        public string Floor { get; }
        public string Room { get; }

        private Location(string building, string floor, string room)
        {
            Building = building;
            Floor = floor;
            Room = room;
        }

        private Location() { } // EF Core

        public static Location Create(string building, string floor, string room)
            => new(building, floor, room);

        public Location ChangeBuilding(string newBuilding)
            => new(newBuilding, Floor, Room);

        public Location ChangeFloor(string newFloor)
            => new(Building, newFloor, Room);

        public Location ChangeRoom(string newRoom)
            => new(Building, Floor, newRoom);

        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return Building;
            yield return Floor;
            yield return Room;
        }

        public override string ToString() => $"{Building}:{Floor}:{Room}";
    }
}
