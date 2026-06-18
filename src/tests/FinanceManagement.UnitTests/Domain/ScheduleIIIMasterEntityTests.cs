using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class ScheduleIIIMasterEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive() =>
            new ScheduleIIIMaster().IsActive.Should().Be(Status.Active);

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted() =>
            new ScheduleIIIMaster().IsDeleted.Should().Be(IsDelete.NotDeleted);

        [Fact]
        public void ShouldInheritFromBaseEntity() =>
            typeof(FinanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ScheduleIIIMaster)).Should().BeTrue();

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new ScheduleIIIMaster
            {
                Id = 1,
                CompanyId = 1001,
                DivisionId = 7,
                StatusId = 120,
                TextileSplitEnabled = true,
                ScheduleIIISectionItemId = 50,
                DisplayOrder = 3
            };

            entity.CompanyId.Should().Be(1001);
            entity.DivisionId.Should().Be(7);
            entity.StatusId.Should().Be(120);
            entity.TextileSplitEnabled.Should().BeTrue();
            entity.ScheduleIIISectionItemId.Should().Be(50);
            entity.DisplayOrder.Should().Be(3);
        }
    }
}
