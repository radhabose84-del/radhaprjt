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
        public void IsSystemDefined_DefaultsToTrue() =>
            new ScheduleIIISubTotal().IsSystemDefined.Should().BeTrue();

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
                CompanyId = 1,
                DivisionId = 7,
                SubTotalTypeId = 29,
                FormulaExpression = "Gross Profit + Other Income - Operating Expenses",
                IncludeOtherIncome = true,
                IsSystemDefined = true,
                DisplayOrder = 2
            };

            entity.SubTotalTypeId.Should().Be(29);
            entity.FormulaExpression.Should().Contain("Other Income");
            entity.IncludeOtherIncome.Should().BeTrue();
            entity.DisplayOrder.Should().Be(2);
        }
    }
}
