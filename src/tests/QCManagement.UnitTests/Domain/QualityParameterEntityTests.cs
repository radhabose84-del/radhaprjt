using QCManagement.Domain.Common;
using QCManagement.Domain.Entities;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.UnitTests.Domain
{
    public class QualityParameterEntityTests
    {
        [Fact]
        public void QualityParameter_DefaultIsActive_ShouldBeActive()
        {
            var entity = new QualityParameter();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void QualityParameter_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new QualityParameter();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void QualityParameter_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(QualityParameter)).Should().BeTrue();
        }

        [Fact]
        public void QualityParameter_Properties_ShouldBeAssignable()
        {
            var entity = new QualityParameter
            {
                Id = 42,
                ParameterCode = "QP-000042",
                ParameterName = "Yarn Tensile Strength",
                ParameterGroupId = 1,
                DataTypeId = 2,
                UnitId = 12,
                ValidationTypeId = 3,
                Description = "Breaking strength"
            };

            entity.Id.Should().Be(42);
            entity.ParameterCode.Should().Be("QP-000042");
            entity.ParameterName.Should().Be("Yarn Tensile Strength");
            entity.ParameterGroupId.Should().Be(1);
            entity.DataTypeId.Should().Be(2);
            entity.UnitId.Should().Be(12);
            entity.ValidationTypeId.Should().Be(3);
            entity.Description.Should().Be("Breaking strength");
        }

        [Fact]
        public void QualityParameter_UnitId_ShouldAcceptNull()
        {
            var entity = new QualityParameter { UnitId = null };
            entity.UnitId.Should().BeNull();
        }

        [Fact]
        public void QualityParameter_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new QualityParameter
            {
                ParameterGroup = new MiscMaster { Code = "MEC" },
                DataType = new MiscMaster { Code = "DEC" },
                ValidationType = new MiscMaster { Code = "RNG" }
            };

            entity.ParameterGroup.Should().NotBeNull();
            entity.DataType.Should().NotBeNull();
            entity.ValidationType.Should().NotBeNull();
        }
    }
}
