using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class AssetMasterGeneralsEntityTests
    {
        [Fact]
        public void AssetMasterGenerals_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AssetMasterGenerals();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AssetMasterGenerals_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AssetMasterGenerals();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AssetMasterGenerals_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AssetMasterGenerals)).Should().BeTrue();
        }

        [Fact]
        public void AssetMasterGenerals_Properties_ShouldBeAssignable()
        {
            var entity = new AssetMasterGenerals
            {
                Id = 1,
                AssetCode = "AST001",
                AssetName = "Test Asset",
                CompanyId = 1,
                UnitId = 1,
                AssetGroupId = 2,
                AssetCategoryId = 3,
                AssetSubCategoryId = 4,
                ISDepreciated = 1,
                IsTangible = 1
            };

            entity.Id.Should().Be(1);
            entity.AssetCode.Should().Be("AST001");
            entity.AssetName.Should().Be("Test Asset");
            entity.CompanyId.Should().Be(1);
            entity.UnitId.Should().Be(1);
        }

        [Fact]
        public void AssetMasterGenerals_NullableProperties_AcceptNull()
        {
            var entity = new AssetMasterGenerals
            {
                AssetSubGroupId = null,
                AssetParentId = null,
                AssetType = null,
                Quantity = null,
                UOMId = null,
                WorkingStatus = null,
                AssetImage = null,
                AssetDocument = null,
                PutToUseDate = null
            };

            entity.AssetSubGroupId.Should().BeNull();
            entity.AssetParentId.Should().BeNull();
            entity.PutToUseDate.Should().BeNull();
        }
    }
}
