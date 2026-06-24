using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class AccountingPeriodEntityTests
    {
        [Fact]
        public void AccountingPeriod_DefaultIsActive_ShouldBeActive()
        {
            new AccountingPeriod().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AccountingPeriod_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new AccountingPeriod().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AccountingPeriod_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AccountingPeriod)).Should().BeTrue();
        }

        [Fact]
        public void AccountingPeriod_Properties_ShouldBeAssignable()
        {
            var entity = new AccountingPeriod
            {
                Id = 1,
                CompanyId = 7,
                FinancialYearId = 3,
                PeriodName = "Jun 2026",
                PeriodNo = 3,
                StartDate = new DateOnly(2026, 6, 1),
                EndDate = new DateOnly(2026, 6, 30),
                StatusId = 121
            };

            entity.Id.Should().Be(1);
            entity.CompanyId.Should().Be(7);
            entity.FinancialYearId.Should().Be(3);
            entity.PeriodName.Should().Be("Jun 2026");
            entity.PeriodNo.Should().Be(3);
            entity.StartDate.Should().Be(new DateOnly(2026, 6, 1));
            entity.EndDate.Should().Be(new DateOnly(2026, 6, 30));
            entity.StatusId.Should().Be(121);
        }
    }
}
