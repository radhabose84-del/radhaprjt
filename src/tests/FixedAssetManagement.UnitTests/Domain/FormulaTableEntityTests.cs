using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class FormulaTableEntityTests
    {
        [Fact]
        public void FormulaTable_DefaultIsActive_ShouldBeActive()
        {
            var entity = new FormulaTable();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void FormulaTable_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new FormulaTable();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void FormulaTable_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(FormulaTable)).Should().BeTrue();
        }

        [Fact]
        public void FormulaTable_Properties_ShouldBeAssignable()
        {
            var entity = new FormulaTable
            {
                Id = 1,
                FormulaName = "SLM",
                FormulaText = "(Cost - Residual) / UsefulLife",
                Description = "Straight line method",
                Type = "Depreciation"
            };

            entity.Id.Should().Be(1);
            entity.FormulaName.Should().Be("SLM");
            entity.FormulaText.Should().Be("(Cost - Residual) / UsefulLife");
            entity.Description.Should().Be("Straight line method");
            entity.Type.Should().Be("Depreciation");
        }

        [Fact]
        public void FormulaTable_NullableProperties_ShouldAcceptNull()
        {
            var entity = new FormulaTable
            {
                FormulaName = null,
                FormulaText = null,
                Description = null,
                Type = null
            };

            entity.FormulaName.Should().BeNull();
            entity.FormulaText.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.Type.Should().BeNull();
        }
    }
}
