using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class FinancialPeriodMasterEntityTests
    {
        [Fact]
        public void FinancialPeriodMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new FinancialPeriodMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void FinancialPeriodMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new FinancialPeriodMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void FinancialPeriodMaster_DefaultIsAdjustmentPeriod_ShouldBeFalse()
        {
            var entity = new FinancialPeriodMaster();
            entity.IsAdjustmentPeriod.Should().BeFalse();
        }

        [Fact]
        public void FinancialPeriodMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(FinancialPeriodMaster)).Should().BeTrue();
        }

        [Fact]
        public void FinancialPeriodMaster_Properties_ShouldBeAssignable()
        {
            var entity = new FinancialPeriodMaster
            {
                Id = 1,
                FinancialYearId = 2,
                CompanyId = 3,
                PeriodNumber = 13,
                PeriodName = "Adj-2024-25",
                StartDate = new DateOnly(2025, 3, 31),
                EndDate = new DateOnly(2025, 3, 31),
                StatusId = 200,
                IsAdjustmentPeriod = true,
                LastStatusChangedBy = 42,
                LastStatusChangedAt = new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero)
            };

            entity.FinancialYearId.Should().Be(2);
            entity.PeriodNumber.Should().Be(13);
            entity.PeriodName.Should().Be("Adj-2024-25");
            entity.IsAdjustmentPeriod.Should().BeTrue();
            entity.LastStatusChangedBy.Should().Be(42);
        }

        [Fact]
        public void FinancialPeriodMaster_AuditPointers_ShouldAcceptNull()
        {
            var entity = new FinancialPeriodMaster
            {
                LastStatusChangedBy = null,
                LastStatusChangedAt = null
            };
            entity.LastStatusChangedBy.Should().BeNull();
            entity.LastStatusChangedAt.Should().BeNull();
        }

        [Fact]
        public void FinancialPeriodMaster_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new FinancialPeriodMaster
            {
                FinancialYear = new FinancialYearMaster { FinancialYearCode = "2024-25" },
                StatusMaster = new MiscMaster { Code = "OPEN" }
            };
            entity.FinancialYear!.FinancialYearCode.Should().Be("2024-25");
            entity.StatusMaster!.Code.Should().Be("OPEN");
        }
    }
}
