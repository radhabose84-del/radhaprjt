using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class ScheduleIIILineItemEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive() =>
            new ScheduleIIILineItem().IsActive.Should().Be(Status.Active);

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted() =>
            new ScheduleIIILineItem().IsDeleted.Should().Be(IsDelete.NotDeleted);

        [Fact]
        public void ShouldInheritFromBaseEntity() =>
            typeof(FinanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ScheduleIIILineItem)).Should().BeTrue();

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new ScheduleIIILineItem
            {
                Id = 14,
                StructureId = 1,
                SectionId = 5,
                ParentLineId = null,
                LineCode = "INV",
                LineName = "Inventories",
                SubClassification = null,
                NoteReference = "Note 8",
                DisplayOrder = 1,
                IsSplitLine = false
            };

            entity.LineCode.Should().Be("INV");
            entity.LineName.Should().Be("Inventories");
            entity.NoteReference.Should().Be("Note 8");
            entity.IsSplitLine.Should().BeFalse();
        }

        [Fact]
        public void NullableParentAndSubClassification_AcceptNull()
        {
            var entity = new ScheduleIIILineItem { ParentLineId = null, SubClassification = null };
            entity.ParentLineId.Should().BeNull();
            entity.SubClassification.Should().BeNull();
        }

        [Fact]
        public void SplitLine_CanBeFlagged()
        {
            var entity = new ScheduleIIILineItem { LineCode = "REV-T", IsSplitLine = true };
            entity.IsSplitLine.Should().BeTrue();
        }
    }
}
