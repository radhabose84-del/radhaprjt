using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class FinancialYearMasterEntityTests
    {
        [Fact]
        public void FinancialYearMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new FinancialYearMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void FinancialYearMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new FinancialYearMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void FinancialYearMaster_DefaultIsTransitionYear_ShouldBeFalse()
        {
            var entity = new FinancialYearMaster();
            entity.IsTransitionYear.Should().BeFalse();
        }

        [Fact]
        public void FinancialYearMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(FinancialYearMaster)).Should().BeTrue();
        }

        [Fact]
        public void FinancialYearMaster_Properties_ShouldBeAssignable()
        {
            var entity = new FinancialYearMaster
            {
                Id = 1,
                CompanyId = 5,
                FinancialYearCode = "2024-25",
                StartDate = new DateOnly(2024, 4, 1),
                EndDate = new DateOnly(2025, 3, 31),
                StatusId = 100,
                IsTransitionYear = true
            };

            entity.Id.Should().Be(1);
            entity.CompanyId.Should().Be(5);
            entity.FinancialYearCode.Should().Be("2024-25");
            entity.StartDate.Should().Be(new DateOnly(2024, 4, 1));
            entity.EndDate.Should().Be(new DateOnly(2025, 3, 31));
            entity.StatusId.Should().Be(100);
            entity.IsTransitionYear.Should().BeTrue();
        }

        [Fact]
        public void FinancialYearMaster_NullableNav_ShouldAcceptNull()
        {
            var entity = new FinancialYearMaster { StatusMaster = null, Periods = null };
            entity.StatusMaster.Should().BeNull();
            entity.Periods.Should().BeNull();
        }

        [Fact]
        public void FinancialYearMaster_PeriodsCollection_ShouldBeAssignable()
        {
            var entity = new FinancialYearMaster
            {
                Periods = new List<FinancialPeriodMaster>
                {
                    new() { PeriodNumber = 1 },
                    new() { PeriodNumber = 13, IsAdjustmentPeriod = true }
                }
            };
            entity.Periods.Should().HaveCount(2);
        }
    }
}
