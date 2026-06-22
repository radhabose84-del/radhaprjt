using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class ProfitCentreEntityTests
    {
        [Fact]
        public void ProfitCentre_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ProfitCentre();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ProfitCentre_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ProfitCentre();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ProfitCentre_DefaultIsRevenueLinked_ShouldBeTrue()
        {
            var entity = new ProfitCentre();
            entity.IsRevenueLinked.Should().BeTrue();
        }

        [Fact]
        public void ProfitCentre_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ProfitCentre)).Should().BeTrue();
        }

        [Fact]
        public void ProfitCentre_Properties_ShouldBeAssignable()
        {
            var entity = new ProfitCentre
            {
                Id = 1,
                CompanyId = 7,
                ProfitCentreCode = "PC-SPIN",
                ProfitCentreName = "Spinning",
                LevelId = 62,
                ParentProfitCentreId = null,
                ResponsibleHeadId = 5,
                MidYearJustification = "Added mid-year"
            };

            entity.Id.Should().Be(1);
            entity.CompanyId.Should().Be(7);
            entity.ProfitCentreCode.Should().Be("PC-SPIN");
            entity.ProfitCentreName.Should().Be("Spinning");
            entity.LevelId.Should().Be(62);
            entity.ParentProfitCentreId.Should().BeNull();
            entity.ResponsibleHeadId.Should().Be(5);
            entity.MidYearJustification.Should().Be("Added mid-year");
        }

        [Fact]
        public void ProfitCentre_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ProfitCentre
            {
                ParentProfitCentreId = null,
                ResponsibleHeadId = null,
                MidYearJustification = null
            };

            entity.ParentProfitCentreId.Should().BeNull();
            entity.ResponsibleHeadId.Should().BeNull();
            entity.MidYearJustification.Should().BeNull();
        }
    }
}
