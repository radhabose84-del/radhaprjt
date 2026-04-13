using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class AdminSecuritySettingsEntityTests
    {
        [Fact]
        public void AdminSecuritySettings_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new AdminSecuritySettings();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void AdminSecuritySettings_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AdminSecuritySettings();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AdminSecuritySettings_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AdminSecuritySettings)).Should().BeTrue();
        }

        [Fact]
        public void AdminSecuritySettings_Properties_ShouldBeAssignable()
        {
            var entity = new AdminSecuritySettings
            {
                Id = 1,
                PasswordHistoryCount = 5,
                SessionTimeoutMinutes = 30,
                MaxFailedLoginAttempts = 3,
                AccountAutoUnlockMinutes = 15,
                PasswordExpiryDays = 90,
                PasswordExpiryAlertDays = 7,
                IsTwoFactorAuthenticationEnabled = 1,
                MaxConcurrentLogins = 2,
                IsForcePasswordChangeOnFirstLogin = 1,
                PasswordResetCodeExpiryMinutes = 10,
                IsCaptchaEnabledOnLogin = 1,
                EntityId = 4
            };

            entity.Id.Should().Be(1);
            entity.PasswordHistoryCount.Should().Be(5);
            entity.SessionTimeoutMinutes.Should().Be(30);
            entity.MaxFailedLoginAttempts.Should().Be(3);
            entity.AccountAutoUnlockMinutes.Should().Be(15);
            entity.PasswordExpiryDays.Should().Be(90);
            entity.PasswordExpiryAlertDays.Should().Be(7);
            entity.IsTwoFactorAuthenticationEnabled.Should().Be((byte)1);
            entity.MaxConcurrentLogins.Should().Be(2);
            entity.IsForcePasswordChangeOnFirstLogin.Should().Be((byte)1);
            entity.PasswordResetCodeExpiryMinutes.Should().Be(10);
            entity.IsCaptchaEnabledOnLogin.Should().Be((byte)1);
            entity.EntityId.Should().Be(4);
        }

        [Fact]
        public void AdminSecuritySettings_NullableEntityId_ShouldAcceptNull()
        {
            var entity = new AdminSecuritySettings
            {
                EntityId = null,
                entity = null
            };

            entity.EntityId.Should().BeNull();
            entity.entity.Should().BeNull();
        }

        [Fact]
        public void AdminSecuritySettings_NavigationProperty_Entity_ShouldBeAssignable()
        {
            var owner = new Entity { Id = 7, EntityName = "Test Entity" };
            var entity = new AdminSecuritySettings
            {
                EntityId = 7,
                entity = owner
            };

            entity.entity.Should().NotBeNull();
            entity.entity!.EntityName.Should().Be("Test Entity");
        }
    }
}
