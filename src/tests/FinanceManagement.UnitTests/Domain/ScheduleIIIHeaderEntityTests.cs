using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class ScheduleIIIHeaderEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive() =>
            new ScheduleIIIHeader().IsActive.Should().Be(Status.Active);

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted() =>
            new ScheduleIIIHeader().IsDeleted.Should().Be(IsDelete.NotDeleted);

        [Fact]
        public void ShouldInheritFromBaseEntity() =>
            typeof(FinanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ScheduleIIIHeader)).Should().BeTrue();

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new ScheduleIIIHeader
            {
                Id = 1,
                CompanyId = 1001,
                DivisionId = 7,
                StatusId = 120,
                TextileSplitEnabled = true
            };

            entity.CompanyId.Should().Be(1001);
            entity.DivisionId.Should().Be(7);
            entity.StatusId.Should().Be(120);
            entity.TextileSplitEnabled.Should().BeTrue();
        }
    }
}
