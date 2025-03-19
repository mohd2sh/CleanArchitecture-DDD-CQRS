using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Assets.ValueObjects
{
    public sealed record AssetLocation(string Site, string Area, string Zone) : ValueObject
    {
        public static AssetLocation Create(string site, string area, string zone)
            => new(site, area, zone);

        public AssetLocation ChangeArea(string newArea)
            => new(Site, newArea, Zone);

        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return Site;
            yield return Area;
            yield return Zone;
        }

        public override string ToString() => $"{Site}-{Area}-{Zone}";
    }
}
