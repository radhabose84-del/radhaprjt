using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class ScheduleIIISubTotalFormulaEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive() =>
            new ScheduleIIISubTotalFormula().IsActive.Should().Be(Status.Active);

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted() =>
            new ScheduleIIISubTotalFormula().IsDeleted.Should().Be(IsDelete.NotDeleted);

        [Fact]
        public void ShouldInheritFromBaseEntity() =>
            typeof(FinanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ScheduleIIISubTotalFormula)).Should().BeTrue();

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new ScheduleIIISubTotalFormula
            {
                Id = 1,
                SubTotalId = 1,
                OperandTypeId = 140,
                OperandRefId = 19,
                OperatorId = 130,
                DisplayOrder = 1
            };

            entity.SubTotalId.Should().Be(1);
            entity.OperandTypeId.Should().Be(140);
            entity.OperandRefId.Should().Be(19);
            entity.OperatorId.Should().Be(130);
            entity.DisplayOrder.Should().Be(1);
        }
    }
}
