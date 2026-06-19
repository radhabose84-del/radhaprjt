using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class CostCentreEntityTests
    {
        [Fact]
        public void CostCentre_DefaultIsActive_ShouldBeActive()
        {
            new CostCentre().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CostCentre_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new CostCentre().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CostCentre_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CostCentre)).Should().BeTrue();
        }

        [Fact]
        public void CostCentre_Properties_ShouldBeAssignable()
        {
            var entity = new CostCentre
            {
                Id = 1,
                UnitId = 5,
                CompanyId = 7,
                CostCentreCode = "PROD-BR-001",
                CostCentreName = "Blow Room",
                CentreLevelId = 61,
                ParentCostCentreId = 2,
                DepartmentGroupId = 13,
                DepartmentId = 80
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(5);
            entity.CompanyId.Should().Be(7);
            entity.CostCentreCode.Should().Be("PROD-BR-001");
            entity.CostCentreName.Should().Be("Blow Room");
            entity.CentreLevelId.Should().Be(61);
            entity.ParentCostCentreId.Should().Be(2);
            entity.DepartmentGroupId.Should().Be(13);
            entity.DepartmentId.Should().Be(80);
        }

        [Fact]
        public void CostCentre_ReservedFields_ShouldAcceptNull()
        {
            var entity = new CostCentre
            {
                ParentCostCentreId = null,
                DepartmentGroupId = null,
                DepartmentId = null,
                ResponsibleManagerId = null,
                EffectiveFromDate = null,
                EffectiveToDate = null
            };

            entity.ParentCostCentreId.Should().BeNull();
            entity.ResponsibleManagerId.Should().BeNull();
            entity.EffectiveFromDate.Should().BeNull();
        }
    }
}
