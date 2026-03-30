using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class DepreciationGroupsEntityTests
    {
        [Fact]
        public void DepreciationGroups_DefaultIsActive_ShouldBeActive()
        {
            var entity = new DepreciationGroups();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void DepreciationGroups_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new DepreciationGroups();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void DepreciationGroups_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(DepreciationGroups)).Should().BeTrue();
        }

        [Fact]
        public void DepreciationGroups_Properties_ShouldBeAssignable()
        {
            var entity = new DepreciationGroups
            {
                Id = 1,
                Code = "DG001",
                DepreciationGroupName = "TestGroup",
                AssetGroupId = 5,
                BookType = 1,
                DepreciationMethod = 2,
                UsefulLife = 10,
                ResidualValue = 5,
                SortOrder = 3,
                DepreciationRate = 10.5m
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("DG001");
            entity.DepreciationGroupName.Should().Be("TestGroup");
            entity.AssetGroupId.Should().Be(5);
            entity.SortOrder.Should().Be(3);
        }

        [Fact]
        public void DepreciationGroups_NullableProperties_ShouldAcceptNull()
        {
            var entity = new DepreciationGroups
            {
                Code = null,
                DepreciationGroupName = null,
                BookType = null,
                DepreciationMethod = null,
                ResidualValue = null
            };

            entity.Code.Should().BeNull();
            entity.DepreciationGroupName.Should().BeNull();
            entity.BookType.Should().BeNull();
        }
    }
}
