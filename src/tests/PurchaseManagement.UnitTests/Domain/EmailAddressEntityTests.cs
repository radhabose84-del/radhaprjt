using PurchaseManagement.Domain.Entities.ValueObjects;

namespace PurchaseManagement.UnitTests.Domain
{
    public class EmailAddressEntityTests
    {
        [Fact]
        public void EmailAddress_ValidEmail_ShouldStoreValue()
        {
            var email = new EmailAddress("test@example.com");
            email.Value.Should().Be("test@example.com");
        }

        [Fact]
        public void EmailAddress_ShouldTrimWhitespace()
        {
            var email = new EmailAddress("  test@example.com  ");
            email.Value.Should().Be("test@example.com");
        }

        [Fact]
        public void EmailAddress_ImplicitConversionToString_ShouldReturnValue()
        {
            var email = new EmailAddress("user@domain.com");
            string result = email;
            result.Should().Be("user@domain.com");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("invalidemail")]
        public void EmailAddress_InvalidEmail_ShouldThrowArgumentException(string? value)
        {
            Action act = () => new EmailAddress(value!);
            act.Should().Throw<ArgumentException>()
               .WithMessage("Invalid email address");
        }

        [Fact]
        public void EmailAddress_IsSealed_Record()
        {
            typeof(EmailAddress).IsSealed.Should().BeTrue();
        }
    }
}
