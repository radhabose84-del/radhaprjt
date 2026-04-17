using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.UnitTests.Domain
{
    public class IndentLogEntityTests
    {
        [Fact]
        public void IndentLog_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(IndentLog)).Should().BeFalse();
        }

        [Fact]
        public void IndentLog_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new IndentLog
            {
                Id = 1,
                IndentHeaderId = 10,
                ActionType = "Approve",
                ActionRemarks = "Approved by manager",
                PreviousData = "{\"status\":\"Draft\"}",
                NewData = "{\"status\":\"Approved\"}",
                StatusId = 2,
                CreatedBy = 1,
                CreatedDate = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1"
            };

            entity.Id.Should().Be(1);
            entity.IndentHeaderId.Should().Be(10);
            entity.ActionType.Should().Be("Approve");
            entity.ActionRemarks.Should().Be("Approved by manager");
            entity.PreviousData.Should().Contain("Draft");
            entity.NewData.Should().Contain("Approved");
            entity.StatusId.Should().Be(2);
            entity.CreatedBy.Should().Be(1);
            entity.CreatedDate.Should().Be(now);
            entity.CreatedByName.Should().Be("admin");
            entity.CreatedIP.Should().Be("127.0.0.1");
        }
    }
}
