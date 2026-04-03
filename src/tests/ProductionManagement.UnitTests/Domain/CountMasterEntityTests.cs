using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class CountMasterEntityTests
    {
        [Fact]
        public void CountMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new CountMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CountMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new CountMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CountMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CountMaster)).Should().BeTrue();
        }

        [Fact]
        public void CountMaster_Properties_ShouldBeAssignable()
        {
            var entity = new CountMaster
            {
                Id = 1,
                CountCode = "CNT001",
                CountValue = 40.5m,
                ShortName = "40s",
                UOMId = 2,
                CountTypeId = 3
            };
            entity.Id.Should().Be(1);
            entity.CountCode.Should().Be("CNT001");
            entity.CountValue.Should().Be(40.5m);
            entity.ShortName.Should().Be("40s");
            entity.UOMId.Should().Be(2);
            entity.CountTypeId.Should().Be(3);
        }

        [Fact]
        public void CountMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new CountMaster
            {
                CountCode = null,
                ShortName = null,
                CountCategoryId = null,
                CountDescription = null
            };
            entity.CountCode.Should().BeNull();
            entity.ShortName.Should().BeNull();
            entity.CountCategoryId.Should().BeNull();
            entity.CountDescription.Should().BeNull();
        }

        [Fact]
        public void CountMaster_NavigationProperties_ShouldAcceptNull()
        {
            var entity = new CountMaster
            {
                CountType = null,
                CountCategory = null
            };
            entity.CountType.Should().BeNull();
            entity.CountCategory.Should().BeNull();
        }

        [Fact]
        public void CountMaster_NavigationProperties_ShouldBeAssignable()
        {
            var countType = new MiscMaster { Id = 1, Description = "Ring" };
            var countCategory = new MiscMaster { Id = 2, Description = "Carded" };

            var entity = new CountMaster
            {
                CountType = countType,
                CountCategory = countCategory
            };

            entity.CountType.Should().BeSameAs(countType);
            entity.CountCategory.Should().BeSameAs(countCategory);
        }
    }
}
