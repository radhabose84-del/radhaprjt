using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class PeriodStatusOverrideEntityTests
    {
        [Fact]
        public void PeriodStatusOverride_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PeriodStatusOverride();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PeriodStatusOverride_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PeriodStatusOverride();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PeriodStatusOverride_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PeriodStatusOverride)).Should().BeTrue();
        }

        [Fact]
        public void PeriodStatusOverride_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new PeriodStatusOverride
            {
                Id = 1,
                FinancialPeriodId = 5,
                CompanyId = 1,
                FromStatusId = 200,
                ToStatusId = 100,
                RequestedBy = 42,
                RequestedAt = now,
                RequestedReason = "Q4 corrections",
                OverrideStatusId = 300
            };

            entity.FinancialPeriodId.Should().Be(5);
            entity.FromStatusId.Should().Be(200);
            entity.ToStatusId.Should().Be(100);
            entity.RequestedBy.Should().Be(42);
            entity.RequestedAt.Should().Be(now);
            entity.RequestedReason.Should().Be("Q4 corrections");
        }

        [Fact]
        public void PeriodStatusOverride_ApproverFields_ShouldDefaultToNull()
        {
            var entity = new PeriodStatusOverride();
            entity.CfoApproverId.Should().BeNull();
            entity.CfoApprovedAt.Should().BeNull();
            entity.SysAdminApproverId.Should().BeNull();
            entity.SysAdminApprovedAt.Should().BeNull();
            entity.AppliedAt.Should().BeNull();
            entity.RejectionReason.Should().BeNull();
        }

        [Fact]
        public void PeriodStatusOverride_ApproverFields_ShouldAcceptValues()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new PeriodStatusOverride
            {
                CfoApproverId = 10,
                CfoApprovedAt = now,
                SysAdminApproverId = 20,
                SysAdminApprovedAt = now.AddMinutes(1)
            };
            entity.CfoApproverId.Should().Be(10);
            entity.SysAdminApproverId.Should().Be(20);
            entity.CfoApprovedAt.Should().Be(now);
        }
    }
}
