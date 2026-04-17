using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class RawMaterialTypeEntityTests
    {
        [Fact]
        public void RawMaterialType_DefaultIsActive_ShouldBeActive()
        {
            var entity = new RawMaterialType();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void RawMaterialType_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new RawMaterialType();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void RawMaterialType_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RawMaterialType)).Should().BeTrue();
        }

        [Fact]
        public void RawMaterialType_Properties_ShouldBeAssignable()
        {
            var effectiveFrom = DateTimeOffset.UtcNow;
            var effectiveTo = effectiveFrom.AddMonths(6);
            var entity = new RawMaterialType
            {
                Id = 1,
                RawMaterialTypeCode = "RMT001",
                RawMaterialTypeName = "Cotton Raw",
                Description = "Raw cotton material",
                EffectiveFrom = effectiveFrom,
                EffectiveTo = effectiveTo
            };
            entity.Id.Should().Be(1);
            entity.RawMaterialTypeCode.Should().Be("RMT001");
            entity.RawMaterialTypeName.Should().Be("Cotton Raw");
            entity.Description.Should().Be("Raw cotton material");
            entity.EffectiveFrom.Should().Be(effectiveFrom);
            entity.EffectiveTo.Should().Be(effectiveTo);
        }

        [Fact]
        public void RawMaterialType_NullableProperties_ShouldAcceptNull()
        {
            var entity = new RawMaterialType
            {
                RawMaterialTypeCode = null,
                RawMaterialTypeName = null,
                Description = null,
                EffectiveTo = null
            };
            entity.RawMaterialTypeCode.Should().BeNull();
            entity.RawMaterialTypeName.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.EffectiveTo.Should().BeNull();
        }
    }
}
