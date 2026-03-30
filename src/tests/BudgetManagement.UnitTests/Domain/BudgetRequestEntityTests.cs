using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Entities;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.Domain
{
    public class BudgetRequestEntityTests
    {
        [Fact]
        public void BudgetRequest_DefaultIsActive_ShouldBeActive()
        {
            var entity = new BudgetRequest();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void BudgetRequest_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new BudgetRequest();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void BudgetRequest_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(BudgetRequest)).Should().BeTrue();
        }

        [Fact]
        public void BudgetRequest_Properties_ShouldBeAssignable()
        {
            var entity = new BudgetRequest
            {
                Id = 1,
                UnitId = 2,
                FinancialYearId = 2025,
                CurrencyId = 3,
                RequestCode = "REQ001",
                RequestTypeId = 4,
                RequestAmount = 5000m,
                StatusId = 1
            };

            entity.Id.Should().Be(1);
            entity.RequestCode.Should().Be("REQ001");
            entity.RequestAmount.Should().Be(5000m);
        }

        [Fact]
        public void BudgetRequest_NullableProperties_ShouldAcceptNull()
        {
            var entity = new BudgetRequest
            {
                RequestCode = null,
                RequestById = null,
                RequestMonthId = null,
                RevisionNumber = null,
                FromDate = null,
                ToDate = null,
                BudgetGroupId = null,
                ProjectId = null,
                WBSId = null,
                Remarks = null,
                ImagePath = null
            };

            entity.RequestCode.Should().BeNull();
            entity.BudgetGroupId.Should().BeNull();
            entity.ProjectId.Should().BeNull();
        }
    }
}
