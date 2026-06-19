using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class ScheduleIIIDetailEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive() =>
            new ScheduleIIIDetail().IsActive.Should().Be(Status.Active);

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted() =>
            new ScheduleIIIDetail().IsDeleted.Should().Be(IsDelete.NotDeleted);

        [Fact]
        public void ShouldInheritFromBaseEntity() =>
            typeof(FinanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ScheduleIIIDetail)).Should().BeTrue();

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new ScheduleIIIDetail
            {
                Id = 1,
                ScheduleIIIHeaderId = 5,
                ScheduleIIISectionId = 9,
                ScheduleIIISectionItemId = 50,
                DisplayOrder = 3
            };

            entity.ScheduleIIIHeaderId.Should().Be(5);
            entity.ScheduleIIISectionId.Should().Be(9);
            entity.ScheduleIIISectionItemId.Should().Be(50);
            entity.DisplayOrder.Should().Be(3);
        }
    }
}
