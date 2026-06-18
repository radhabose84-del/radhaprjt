using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class ScheduleIIISubTotalEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive() =>
            new ScheduleIIISubTotal().IsActive.Should().Be(Status.Active);

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted() =>
            new ScheduleIIISubTotal().IsDeleted.Should().Be(IsDelete.NotDeleted);

        [Fact]
        public void ShouldInheritFromBaseEntity() =>
            typeof(FinanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ScheduleIIISubTotal)).Should().BeTrue();

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new ScheduleIIISubTotal
            {
                Id = 2,
                FormulaName = "EBITDA",
                FormulaExpression = "Gross Profit + Other Income - Operating Expenses",
                IncludeOtherIncome = true,
                DisplayOrder = 2
            };

            entity.FormulaName.Should().Be("EBITDA");
            entity.FormulaExpression.Should().Contain("Other Income");
            entity.IncludeOtherIncome.Should().BeTrue();
            entity.DisplayOrder.Should().Be(2);
        }
    }
}
