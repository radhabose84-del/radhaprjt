using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class EmailconfigEntityTests
    {
        [Fact]
        public void Emailconfig_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new Emailconfig();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Emailconfig_DefaultIsActive_ShouldBeZero()
        {
            // Emailconfig hides the base IsActive with `new byte IsActive` — defaults to 0
            var entity = new Emailconfig();
            entity.IsActive.Should().Be((byte)0);
        }

        [Fact]
        public void Emailconfig_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(Emailconfig)).Should().BeTrue();
        }

        [Fact]
        public void Emailconfig_Properties_ShouldBeAssignable()
        {
            var sentDate = new DateTime(2025, 6, 15, 10, 30, 0);

            var entity = new Emailconfig
            {
                Id = 1,
                Subject = "Welcome",
                FromAddress = "noreply@bsoft.com",
                ToAddress = "user@bsoft.com",
                CcAddress = "cc@bsoft.com",
                BccAddress = "bcc@bsoft.com",
                MailType = "Notification",
                MailTime = "10:30",
                MailDay = 15,
                MailMonth = 6,
                SentDate = sentDate,
                IsActive = 1
            };

            entity.Id.Should().Be(1);
            entity.Subject.Should().Be("Welcome");
            entity.FromAddress.Should().Be("noreply@bsoft.com");
            entity.ToAddress.Should().Be("user@bsoft.com");
            entity.CcAddress.Should().Be("cc@bsoft.com");
            entity.BccAddress.Should().Be("bcc@bsoft.com");
            entity.MailType.Should().Be("Notification");
            entity.MailTime.Should().Be("10:30");
            entity.MailDay.Should().Be(15);
            entity.MailMonth.Should().Be(6);
            entity.SentDate.Should().Be(sentDate);
            entity.IsActive.Should().Be((byte)1);
        }

        [Fact]
        public void Emailconfig_NullableProperties_ShouldAcceptNull()
        {
            var entity = new Emailconfig
            {
                Subject = null,
                FromAddress = null,
                ToAddress = null,
                CcAddress = null,
                BccAddress = null,
                MailType = null,
                MailTime = null
            };

            entity.Subject.Should().BeNull();
            entity.FromAddress.Should().BeNull();
            entity.ToAddress.Should().BeNull();
            entity.CcAddress.Should().BeNull();
            entity.BccAddress.Should().BeNull();
            entity.MailType.Should().BeNull();
            entity.MailTime.Should().BeNull();
        }

        [Fact]
        public void Emailconfig_IsDeleted_CanBeSetToDeleted()
        {
            var entity = new Emailconfig { IsDeleted = IsDelete.Deleted };
            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
