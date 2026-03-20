using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class LanguageEntityTests
    {
        [Fact]
        public void Language_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new Language();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Language_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new Language();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Language_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(Language)).Should().BeTrue();
        }

        [Fact]
        public void Language_Properties_ShouldBeAssignable()
        {
            var entity = new Language
            {
                Id = 1,
                Code = "EN",
                Name = "English"
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("EN");
            entity.Name.Should().Be("English");
        }

        [Fact]
        public void Language_NullableProperties_ShouldAcceptNull()
        {
            var entity = new Language
            {
                Code = null,
                Name = null,
                CompanySettings = null
            };

            entity.Code.Should().BeNull();
            entity.Name.Should().BeNull();
            entity.CompanySettings.Should().BeNull();
        }
    }
}
