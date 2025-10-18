using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Assets.ValueObjects
{
    internal sealed record AssetLocation : ValueObject
    {
        public string Site { get; private set; }
        public string Area { get; private set; }
        public string Zone { get; private set; }

        private AssetLocation(string site, string area, string zone)
        {
            Site = site;
            Area = area;
            Zone = zone;
        }

        private AssetLocation() { } // EF Core parameterless ctor

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
