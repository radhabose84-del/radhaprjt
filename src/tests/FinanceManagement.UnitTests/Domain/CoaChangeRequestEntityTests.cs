using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class CoaChangeRequestEntityTests
    {
        [Fact]
        public void CoaChangeRequest_DefaultIsActive_ShouldBeActive()
        {
            new FinanceManagement.Domain.Entities.CoaChangeRequest().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CoaChangeRequest_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new FinanceManagement.Domain.Entities.CoaChangeRequest().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CoaChangeRequest_DefaultStatus_ShouldBePendingImpactApproval()
        {
            new FinanceManagement.Domain.Entities.CoaChangeRequest().RequestStatus.Should().Be(CoaChangeRequestStatus.PendingImpactApproval);
        }

        [Fact]
        public void CoaChangeRequest_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(FinanceManagement.Domain.Entities.CoaChangeRequest)).Should().BeTrue();
        }

        [Fact]
        public void CoaChangeRequest_Properties_ShouldBeAssignable()
        {
            var e = new FinanceManagement.Domain.Entities.CoaChangeRequest
            {
                Id = 1,
                CompanyId = 2,
                TargetAccountId = 3,
                TargetAccountGroupId = 4,
                AccountCodeSnapshot = "1001",
                ChangeType = CoaChangeType.CodeChange,
                Justification = "j",
                ImpactAssessment = "i",
                ImpactApprovedByUserId = 7,
                UnfreezeRequestId = 50,
                IsPostFreeze = true,
                CommittedByUserId = 9,
                RequestedByUserId = 3
            };

            e.TargetAccountId.Should().Be(3);
            e.UnfreezeRequestId.Should().Be(50);
            e.IsPostFreeze.Should().BeTrue();
            e.ChangeType.Should().Be(CoaChangeType.CodeChange);
        }

        [Fact]
        public void CoaChangeRequest_NullableProperties_ShouldAcceptNull()
        {
            var e = new FinanceManagement.Domain.Entities.CoaChangeRequest
            {
                TargetAccountId = null,
                TargetAccountGroupId = null,
                ImpactApprovedByUserId = null,
                ImpactApprovedOn = null,
                UnfreezeRequestId = null,
                CommittedByUserId = null,
                CommittedOn = null
            };

            e.TargetAccountId.Should().BeNull();
            e.UnfreezeRequestId.Should().BeNull();
            e.CommittedOn.Should().BeNull();
        }
    }
}
