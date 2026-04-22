using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.UnitTests.Domain
{
    public class ApprovalRequestLineEntityTests
    {
        [Fact]
        public void ApprovalRequestLine_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new ApprovalRequestLine
            {
                Id = 1,
                ApprovalRequestId = 10,
                ModuleLineTransactionId = 200,
                StatusId = 5,
                Remark = "Line approved",
                ModifiedBy = 3,
                ModifiedDate = now,
                ModifiedByName = "reviewer",
                ModifiedIP = "10.0.0.1"
            };

            entity.Id.Should().Be(1);
            entity.ApprovalRequestId.Should().Be(10);
            entity.ModuleLineTransactionId.Should().Be(200);
            entity.StatusId.Should().Be(5);
            entity.Remark.Should().Be("Line approved");
            entity.ModifiedBy.Should().Be(3);
            entity.ModifiedDate.Should().Be(now);
            entity.ModifiedByName.Should().Be("reviewer");
            entity.ModifiedIP.Should().Be("10.0.0.1");
        }

        [Fact]
        public void ApprovalRequestLine_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ApprovalRequestLine
            {
                ModifiedBy = null,
                ModifiedDate = null,
                ModifiedByName = null,
                ModifiedIP = null
            };

            entity.ModifiedBy.Should().BeNull();
            entity.ModifiedDate.Should().BeNull();
            entity.ModifiedByName.Should().BeNull();
            entity.ModifiedIP.Should().BeNull();
        }

        [Fact]
        public void ApprovalRequestLine_NavigationProperties_ShouldBeAssignable()
        {
            var request = new ApprovalRequest { Id = 10 };
            var status = new MiscMaster { Id = 5 };

            var entity = new ApprovalRequestLine
            {
                ApprovalRequestId = 10,
                ApprovalRequest = request,
                StatusId = 5,
                Status = status
            };

            entity.ApprovalRequest.Should().NotBeNull();
            entity.ApprovalRequest.Id.Should().Be(10);
            entity.Status.Should().NotBeNull();
            entity.Status.Id.Should().Be(5);
        }

        [Fact]
        public void ApprovalRequestLine_Id_DefaultShouldBeZero()
        {
            var entity = new ApprovalRequestLine();
            entity.Id.Should().Be(0);
        }
    }
}
