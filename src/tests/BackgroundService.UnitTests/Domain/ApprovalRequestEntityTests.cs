using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.UnitTests.Domain
{
    public class ApprovalRequestEntityTests
    {
        [Fact]
        public void ApprovalRequest_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new ApprovalRequest
            {
                Id = 1,
                WorkflowType = "Purchase",
                WorkflowTypeId = 2,
                ModuleTransactionId = 100,
                ApprovalStepDetailId = 5,
                ApprovalRuleId = 3,
                StatusId = 10,
                RequestedDate = now,
                UnitId = 7,
                DepartmentId = 4,
                Remark = "Pending review",
                ModifiedBy = 6,
                ModifiedDate = now.AddHours(1),
                ModifiedByName = "editor",
                ModifiedIP = "192.168.1.1",
                Action = "Approve",
                ApproverBinding = "Role",
                ApproverValue = "Manager"
            };

            entity.Id.Should().Be(1);
            entity.WorkflowType.Should().Be("Purchase");
            entity.WorkflowTypeId.Should().Be(2);
            entity.ModuleTransactionId.Should().Be(100);
            entity.ApprovalStepDetailId.Should().Be(5);
            entity.ApprovalRuleId.Should().Be(3);
            entity.StatusId.Should().Be(10);
            entity.RequestedDate.Should().Be(now);
            entity.UnitId.Should().Be(7);
            entity.DepartmentId.Should().Be(4);
            entity.Remark.Should().Be("Pending review");
            entity.ModifiedBy.Should().Be(6);
            entity.ModifiedDate.Should().Be(now.AddHours(1));
            entity.ModifiedByName.Should().Be("editor");
            entity.ModifiedIP.Should().Be("192.168.1.1");
            entity.Action.Should().Be("Approve");
            entity.ApproverBinding.Should().Be("Role");
            entity.ApproverValue.Should().Be("Manager");
        }

        [Fact]
        public void ApprovalRequest_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ApprovalRequest
            {
                ApprovalRuleId = null,
                DepartmentId = null,
                ModifiedBy = null,
                ModifiedDate = null,
                ModifiedByName = null,
                ModifiedIP = null,
                Action = null
            };

            entity.ApprovalRuleId.Should().BeNull();
            entity.DepartmentId.Should().BeNull();
            entity.ModifiedBy.Should().BeNull();
            entity.ModifiedDate.Should().BeNull();
            entity.ModifiedByName.Should().BeNull();
            entity.ModifiedIP.Should().BeNull();
            entity.Action.Should().BeNull();
        }

        [Fact]
        public void ApprovalRequest_NavigationProperties_ShouldBeAssignable()
        {
            var stepDetail = new ApprovalStepDetail { Id = 5 };
            var rule = new ApprovalRule { Id = 3 };
            var status = new MiscMaster { Id = 10 };
            var docs = new List<ApprovalDocument> { new ApprovalDocument { Id = 1 } };
            var lines = new List<ApprovalRequestLine> { new ApprovalRequestLine { Id = 1 } };

            var entity = new ApprovalRequest
            {
                ApprovalStepDetail = stepDetail,
                ApprovalRule = rule,
                Status = status,
                ApprovalDocuments = docs,
                ApprovalRequestLines = lines
            };

            entity.ApprovalStepDetail.Should().NotBeNull();
            entity.ApprovalStepDetail.Id.Should().Be(5);
            entity.ApprovalRule.Should().NotBeNull();
            entity.Status.Should().NotBeNull();
            entity.ApprovalDocuments.Should().HaveCount(1);
            entity.ApprovalRequestLines.Should().HaveCount(1);
        }

        [Fact]
        public void ApprovalRequest_Id_DefaultShouldBeZero()
        {
            var entity = new ApprovalRequest();
            entity.Id.Should().Be(0);
        }
    }
}
