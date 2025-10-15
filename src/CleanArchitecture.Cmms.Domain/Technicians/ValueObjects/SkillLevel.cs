using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Technicians.ValueObjects
{
    internal sealed record SkillLevel(string LevelName, int Rank) : ValueObject
    {
        public static SkillLevel Apprentice => new("Apprentice", 1);
        public static SkillLevel Journeyman => new("Journeyman", 2);
        public static SkillLevel Master => new("Master", 3);

        public bool IsHigherThan(SkillLevel other) => Rank > other.Rank;

        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return LevelName;
            yield return Rank;
        }

        public override string ToString() => LevelName;
    }
}
