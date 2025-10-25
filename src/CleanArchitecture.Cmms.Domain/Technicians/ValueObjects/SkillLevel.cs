using CleanArchitecture.Core.Domain.Abstractions  ;

namespace CleanArchitecture.Cmms.Domain.Technicians.ValueObjects
{
    internal sealed record SkillLevel : ValueObject
    {
        public string LevelName { get; private set; } = default!;
        public int Rank { get; private set; }

        private SkillLevel(string levelName, int rank)
        {
            LevelName = levelName;
            Rank = rank;
        }

        private SkillLevel() { }

        public static SkillLevel Apprentice => new("Apprentice", 1);
        public static SkillLevel Journeyman => new("Journeyman", 2);
        public static SkillLevel Master => new("Master", 3);

        public static SkillLevel Create(string levelName, int rank)
        {
            if (string.IsNullOrWhiteSpace(levelName))
                throw new DomainException(TechnicianErrors.LevelNameRequired);

            if (rank <= 0)
                throw new DomainException(TechnicianErrors.InvalidRank);

            return new SkillLevel(levelName.Trim(), rank);
        }

        public bool IsHigherThan(SkillLevel other) => Rank > other.Rank;

        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return LevelName;
            yield return Rank;
        }

        public override string ToString() => LevelName;
    }
}
