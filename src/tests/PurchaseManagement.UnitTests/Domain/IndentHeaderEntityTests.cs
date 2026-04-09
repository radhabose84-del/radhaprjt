using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class IndentHeaderEntityTests
    {
        [Fact]
        public void IndentHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new IndentHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void IndentHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new IndentHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void IndentHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(IndentHeader)).Should().BeTrue();
        }

        [Fact]
        public void IndentHeader_Properties_ShouldBeAssignable()
        {
            var entity = new IndentHeader
            {
                Id = 1,
                IndentNumber = "IND001",
                IndentDate = DateOnly.FromDateTime(DateTime.Today),
                IndentTypeId = 2,
                UnitId = 3,
                Purpose = "Test Purpose",
                DepartmentId = 4,
                StatusId = 5,
                IndentDetails = new List<IndentDetail>()
            };

            entity.Id.Should().Be(1);
            entity.IndentNumber.Should().Be("IND001");
            entity.IndentTypeId.Should().Be(2);
            entity.UnitId.Should().Be(3);
            entity.Purpose.Should().Be("Test Purpose");
            entity.DepartmentId.Should().Be(4);
            entity.StatusId.Should().Be(5);
        }

        [Fact]
        public void IndentHeader_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new IndentHeader
            {
                IndentDetails = new List<IndentDetail>
                {
                    new IndentDetail { Id = 1 }
                }
            };

            entity.IndentDetails.Should().HaveCount(1);
        }
    }
}
