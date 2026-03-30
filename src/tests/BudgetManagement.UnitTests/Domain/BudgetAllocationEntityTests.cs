using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Entities;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.Domain
{
    public class BudgetAllocationEntityTests
    {
        [Fact]
        public void BudgetAllocation_DefaultIsActive_ShouldBeActive()
        {
            var entity = new BudgetAllocation();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void BudgetAllocation_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new BudgetAllocation();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void BudgetAllocation_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(BudgetAllocation)).Should().BeTrue();
        }

        [Fact]
        public void BudgetAllocation_Properties_ShouldBeAssignable()
        {
            var entity = new BudgetAllocation
            {
                Id = 1,
                FinancialYearId = 2025,
                RequestById = 1,
                UnitId = 5,
                ApprovedAmount = 1000m,
                SpindleCount = 100,
                RatePerSpindle = 10m
            };

            entity.Id.Should().Be(1);
            entity.FinancialYearId.Should().Be(2025);
            entity.ApprovedAmount.Should().Be(1000m);
            entity.SpindleCount.Should().Be(100);
        }

        [Fact]
        public void BudgetAllocation_NullableProperties_ShouldAcceptNull()
        {
            var entity = new BudgetAllocation
            {
                RequestMonthId = null,
                RequestId = null,
                BudgetGroupId = null,
                BudgetSubGroupId = null,
                SpindleCount = null,
                RatePerSpindle = null,
                FromDate = null,
                ToDate = null,
                Remarks = null,
                RemainingBalance = null,
                ProjectId = null,
                WBSId = null
            };

            entity.RequestMonthId.Should().BeNull();
            entity.BudgetGroupId.Should().BeNull();
            entity.SpindleCount.Should().BeNull();
        }
    }
}
