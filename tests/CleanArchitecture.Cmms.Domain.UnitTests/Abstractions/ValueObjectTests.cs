using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Abstractions
{
    public class ValueObjectTests
    {

        [Fact]
        public void Equals_Should_Return_True_For_Same_Values()
        {
            // Arrange
            var money1 = new Money(100m, "USD");
            var money2 = new Money(100m, "USD");

            // Act
            bool result = money1.Equals(money2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Equals_Should_Return_False_For_Null()
        {
            // Arrange
            var money = new Money(100m, "USD");

            // Act
            bool result = money.Equals(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_Should_Return_False_For_Different_Type()
        {
            // Arrange
            var money = new Money(100m, "USD");
            var simple = new SimpleValue(5);

            // Act
            bool result = money.Equals(simple);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_Should_Return_False_When_Lengths_Differ()
        {
            // Arrange
            var money = new Money(100m, "USD");
            var different = new SimpleValue(100);

            // Act
            bool result = money.Equals(different);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_Should_Return_False_When_One_Value_Is_Null()
        {
            // Arrange
            var left = new Money(100m, null!);
            var right = new Money(100m, "USD");

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_Should_Return_False_When_Values_Differ()
        {
            // Arrange
            var left = new Money(100m, "USD");
            var right = new Money(200m, "USD");

            // Act
            bool result = left.Equals(right);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetHashCode_Should_Return_Same_For_Equal_Values()
        {
            // Arrange
            var money1 = new Money(100m, "USD");
            var money2 = new Money(100m, "USD");

            // Act
            int hash1 = money1.GetHashCode();
            int hash2 = money2.GetHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void GetHashCode_Should_Include_Null_Value()
        {
            // Arrange
            var money = new Money(100m, null!);

            // Act
            int hash = money.GetHashCode();

            // Assert
            Assert.IsType<int>(hash);
        }

        [Fact]
        public void EqualOperator_Should_Return_True_For_Both_Null()
        {
            // Arrange
            Money? left = null;
            Money? right = null;

            // Act
            bool result = Money.AreEqual(left, right);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EqualOperator_Should_Return_False_When_Only_One_Null()
        {
            // Arrange
            Money? left = new Money(100m, "USD");
            Money? right = null;

            // Act
            bool result = Money.AreEqual(left, right);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualOperator_Should_Use_Equals_When_Not_Null()
        {
            // Arrange
            var left = new Money(100m, "USD");
            var right = new Money(100m, "USD");

            // Act
            bool result = Money.AreEqual(left, right);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void NotEqualOperator_Should_Return_Opposite_Of_EqualOperator()
        {
            // Arrange
            var left = new Money(100m, "USD");
            var right = new Money(200m, "USD");

            // Act
            bool result = Money.AreNotEqual(left, right);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Equality_Operators_Should_Work_As_Expected()
        {
            // Arrange
            var left = new Money(100m, "USD");
            var right = new Money(100m, "USD");
            var diff = new Money(200m, "USD");

            // Act & Assert
            Assert.True(left == right);
            Assert.False(left != right);
            Assert.False(left == diff);
            Assert.True(left != diff);
        }
    }

    internal record Money(decimal Amount, string Currency) : ValueObject
    {
        public static bool AreEqual(Money? left, Money? right) => EqualOperator(left, right);

        public static bool AreNotEqual(Money? left, Money? right)
            => NotEqualOperator(left, right);

        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    internal record SimpleValue(int X) : ValueObject
    {
        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return X;
        }
    }

}
