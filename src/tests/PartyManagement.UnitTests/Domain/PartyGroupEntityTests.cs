using PartyManagement.Domain.Common;
using PartyManagement.Domain.Entities;
using static PartyManagement.Domain.Common.BaseEntity;
using Xunit;

namespace PartyManagement.UnitTests.Domain
{
    public class PartyGroupEntityTests
    {
        [Fact]
        public void PartyGroup_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PartyGroup();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PartyGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PartyGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PartyGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PartyGroup)).Should().BeTrue();
        }

        [Fact]
        public void PartyGroup_Properties_ShouldBeAssignable()
        {
            var entity = new PartyGroup
            {
                Id = 1,
                PartyGroupName = "Test Group",
                GroupTypeId = 2,
                GlCategoryId = 3,
                IsGroup = true,
                Description = "Test description",
                Glcode = "GL001"
            };

            entity.Id.Should().Be(1);
            entity.PartyGroupName.Should().Be("Test Group");
            entity.GroupTypeId.Should().Be(2);
            entity.GlCategoryId.Should().Be(3);
            entity.IsGroup.Should().BeTrue();
        }

        [Fact]
        public void PartyGroup_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PartyGroup
            {
                PartyGroupName = null,
                ParentPartyGroupId = null,
                Description = null,
                Glcode = null,
                ParentPartyGroup = null,
                ChildPartyGroups = null,
                PartyTypeGroups = null
            };

            entity.PartyGroupName.Should().BeNull();
            entity.ParentPartyGroupId.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
