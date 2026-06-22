using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class CoaUnfreezeRequestEntityTests
    {
        [Fact]
        public void CoaUnfreezeRequest_DefaultIsActive_ShouldBeActive()
        {
            new CoaUnfreezeRequest().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CoaUnfreezeRequest_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new CoaUnfreezeRequest().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CoaUnfreezeRequest_DefaultStatus_ShouldBePendingApproval()
        {
            new CoaUnfreezeRequest().RequestStatus.Should().Be(CoaUnfreezeRequestStatus.PendingApproval);
        }

        [Fact]
        public void CoaUnfreezeRequest_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CoaUnfreezeRequest)).Should().BeTrue();
        }

        [Fact]
        public void CoaUnfreezeRequest_DistinctApproverSlots_ShouldBeAssignable()
        {
            var e = new CoaUnfreezeRequest
            {
                Id = 1,
                CompanyId = 2,
                Reason = "year-end",
                CfoApproverUserId = 7,
                SysAdminApproverUserId = 9,
                WindowMinutes = 60,
                RequestedByUserId = 3
            };

            e.CfoApproverUserId.Should().Be(7);
            e.SysAdminApproverUserId.Should().Be(9);
            e.CfoApproverUserId.Should().NotBe(e.SysAdminApproverUserId);
        }

        [Fact]
        public void CoaUnfreezeRequest_NullableProperties_ShouldAcceptNull()
        {
            var e = new CoaUnfreezeRequest
            {
                CfoApproverUserId = null,
                CfoApprovedOn = null,
                SysAdminApproverUserId = null,
                SysAdminApprovedOn = null,
                WindowOpenedOn = null,
                WindowExpiry = null
            };

            e.CfoApproverUserId.Should().BeNull();
            e.WindowExpiry.Should().BeNull();
        }
    }
}
