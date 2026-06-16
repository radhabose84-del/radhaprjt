using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class ScheduleIIIStructureEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive() =>
            new ScheduleIIIStructure().IsActive.Should().Be(Status.Active);

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted() =>
            new ScheduleIIIStructure().IsDeleted.Should().Be(IsDelete.NotDeleted);

        [Fact]
        public void VersionNo_DefaultsToOne() =>
            new ScheduleIIIStructure().VersionNo.Should().Be(1);

        [Fact]
        public void ShouldInheritFromBaseEntity() =>
            typeof(FinanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ScheduleIIIStructure)).Should().BeTrue();

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new ScheduleIIIStructure
            {
                Id = 1,
                CompanyId = 1001,
                DivisionId = 7,
                StructureStatusId = 120,
                TextileSplitEnabled = true,
                VersionNo = 5
            };

            entity.CompanyId.Should().Be(1001);
            entity.DivisionId.Should().Be(7);
            entity.StructureStatusId.Should().Be(120);
            entity.TextileSplitEnabled.Should().BeTrue();
            entity.VersionNo.Should().Be(5);
        }
    }
}
