using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class SendEmailRequestEntityTests
    {
        [Fact]
        public void SendEmailRequest_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(SendEmailRequest)).Should().BeFalse();
        }

        [Fact]
        public void SendEmailRequest_Properties_ShouldBeAssignable()
        {
            var entity = new SendEmailRequest
            {
                ToEmail = "user@test.com",
                Subject = "Test Subject",
                Body = "<p>Test Body</p>"
            };

            entity.ToEmail.Should().Be("user@test.com");
            entity.Subject.Should().Be("Test Subject");
            entity.Body.Should().Be("<p>Test Body</p>");
        }

        [Fact]
        public void SendEmailRequest_NullableProperties_ShouldAcceptNull()
        {
            var entity = new SendEmailRequest
            {
                ToEmail = null,
                Subject = null,
                Body = null
            };

            entity.ToEmail.Should().BeNull();
            entity.Subject.Should().BeNull();
            entity.Body.Should().BeNull();
        }
    }
}
