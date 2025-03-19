using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects
{
    public sealed record Location(string Building, string Floor, string Room) : ValueObject
    {
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
