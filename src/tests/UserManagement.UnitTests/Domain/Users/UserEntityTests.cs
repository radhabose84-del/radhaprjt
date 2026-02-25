using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain.Users
{
    public class UserEntityTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetPassword_WhenNullOrWhitespace_Throws(string? bad)
        {
            // arrange
            var user = new User();

            // act
            Action act = () => user.SetPassword(bad!);

            // assert
            act.Should().Throw<ArgumentException>()
               .WithParameterName("password")
               .WithMessage("*cannot be null or empty*");
        }

        [Fact]
        public void SetPassword_Stores_BCrypt_Hash_That_Verifies()
        {
            // arrange
            var user = new User();
            var plain = "Secur3P@ss!";

            // act
            user.SetPassword(plain);

            // assert
            user.PasswordHash.Should().NotBeNullOrWhiteSpace();
            // BCrypt hashes typically start with $2a$, $2b$, or $2y$
            user.PasswordHash!.StartsWith("$2").Should().BeTrue("BCrypt hashes start with $2*");
            BCrypt.Net.BCrypt.Verify(plain, user.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public void SetPassword_With_Different_Inputs_Produces_Different_Hashes()
        {
            // arrange
            var user = new User();

            // act
            user.SetPassword("First#123");
            var firstHash = user.PasswordHash!;
            user.SetPassword("Second#456");
            var secondHash = user.PasswordHash!;

            // assert
            firstHash.Should().NotBe(secondHash);
            BCrypt.Net.BCrypt.Verify("First#123", firstHash).Should().BeTrue();
            BCrypt.Net.BCrypt.Verify("Second#456", secondHash).Should().BeTrue();
        }

        [Fact]
        public void SetPassword_Can_Reset_To_New_Value()
        {
            // arrange
            var user = new User();
            user.SetPassword("Old#Pass1");
            var oldHash = user.PasswordHash!;

            // act
            user.SetPassword("New#Pass2");

            // assert
            user.PasswordHash.Should().NotBeNullOrWhiteSpace();
            user.PasswordHash!.Should().NotBe(oldHash);
            BCrypt.Net.BCrypt.Verify("New#Pass2", user.PasswordHash).Should().BeTrue();
            BCrypt.Net.BCrypt.Verify("Old#Pass1", user.PasswordHash).Should().BeFalse();
        }
    }
}
