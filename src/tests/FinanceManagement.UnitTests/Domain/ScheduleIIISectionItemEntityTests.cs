using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class ScheduleIIISectionItemEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive() =>
            new ScheduleIIISectionItem().IsActive.Should().Be(Status.Active);

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted() =>
            new ScheduleIIISectionItem().IsDeleted.Should().Be(IsDelete.NotDeleted);

        [Fact]
        public void ShouldInheritFromBaseEntity() =>
            typeof(FinanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ScheduleIIISectionItem)).Should().BeTrue();

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new ScheduleIIISectionItem
            {
                Id = 14,
                SectionId = 5,
                LineCode = "INV",
                LineName = "Inventories",
                NoteReference = "Note 8",
                IsSplitLine = false
            };

            entity.SectionId.Should().Be(5);
            entity.LineCode.Should().Be("INV");
            entity.LineName.Should().Be("Inventories");
            entity.NoteReference.Should().Be("Note 8");
            entity.IsSplitLine.Should().BeFalse();
        }

        [Fact]
        public void SplitLine_CanBeFlagged()
        {
            var entity = new ScheduleIIISectionItem { LineCode = "REV-T", IsSplitLine = true };
            entity.IsSplitLine.Should().BeTrue();
        }
    }
}
