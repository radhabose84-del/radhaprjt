using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class PackTypeEntityTests
    {
        [Fact]
        public void PackType_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PackType();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PackType_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PackType();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PackType_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PackType)).Should().BeTrue();
        }

        [Fact]
        public void PackType_DefaultProductionAllowed_ShouldBeTrue()
        {
            var entity = new PackType();
            entity.ProductionAllowed.Should().BeTrue();
        }

        [Fact]
        public void PackType_Properties_ShouldBeAssignable()
        {
            var entity = new PackType
            {
                Id = 1,
                PackTypeCode = "PT001",
                PackTypeName = "Cone Bag",
                NetWeight = 50.0m,
                TareWeight = 2.5m,
                GrossWeight = 52.5m,
                ProductionAllowed = true
            };
            entity.Id.Should().Be(1);
            entity.PackTypeCode.Should().Be("PT001");
            entity.PackTypeName.Should().Be("Cone Bag");
            entity.NetWeight.Should().Be(50.0m);
            entity.TareWeight.Should().Be(2.5m);
            entity.GrossWeight.Should().Be(52.5m);
            entity.ProductionAllowed.Should().BeTrue();
        }

        [Fact]
        public void PackType_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PackType
            {
                PackTypeCode = null,
                PackTypeName = null,
                ConesPerBag = null,
                PackMaterialId = null
            };
            entity.PackTypeCode.Should().BeNull();
            entity.PackTypeName.Should().BeNull();
            entity.ConesPerBag.Should().BeNull();
            entity.PackMaterialId.Should().BeNull();
        }

        [Fact]
        public void PackType_NavigationProperty_ShouldAcceptNull()
        {
            var entity = new PackType
            {
                PackMaterial = null,
                ProductionPackDetails = null
            };
            entity.PackMaterial.Should().BeNull();
            entity.ProductionPackDetails.Should().BeNull();
        }

        [Fact]
        public void PackType_NavigationProperty_ShouldBeAssignable()
        {
            var packMaterial = new MiscMaster { Id = 5, Description = "Polypropylene" };
            var entity = new PackType { PackMaterial = packMaterial };
            entity.PackMaterial.Should().BeSameAs(packMaterial);
        }
    }
}
