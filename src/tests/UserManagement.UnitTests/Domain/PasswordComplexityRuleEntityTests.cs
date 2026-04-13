using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class PasswordComplexityRuleEntityTests
    {
        [Fact]
        public void PasswordComplexityRule_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new PasswordComplexityRule();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void PasswordComplexityRule_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PasswordComplexityRule();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PasswordComplexityRule_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PasswordComplexityRule)).Should().BeTrue();
        }

        [Fact]
        public void PasswordComplexityRule_Properties_ShouldBeAssignable()
        {
            var entity = new PasswordComplexityRule
            {
                Id = 1,
                PwdComplexityRule = "Minimum 8 characters, 1 uppercase, 1 digit"
            };

            entity.Id.Should().Be(1);
            entity.PwdComplexityRule.Should().Be("Minimum 8 characters, 1 uppercase, 1 digit");
        }

        [Fact]
        public void PasswordComplexityRule_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PasswordComplexityRule
            {
                PwdComplexityRule = null
            };

            entity.PwdComplexityRule.Should().BeNull();
        }

        [Fact]
        public void PasswordComplexityRule_IsActive_CanBeSetToActive()
        {
            var entity = new PasswordComplexityRule { IsActive = Status.Active };
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PasswordComplexityRule_IsDeleted_CanBeSetToDeleted()
        {
            var entity = new PasswordComplexityRule { IsDeleted = IsDelete.Deleted };
            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
