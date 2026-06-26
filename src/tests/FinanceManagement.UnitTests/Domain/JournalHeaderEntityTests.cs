using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class JournalHeaderEntityTests
    {
        [Fact]
        public void JournalHeader_DefaultIsActive_ShouldBeActive()
        {
            new JournalHeader().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void JournalHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new JournalHeader().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void JournalHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(JournalHeader)).Should().BeTrue();
        }

        [Fact]
        public void JournalHeader_Properties_ShouldBeAssignable()
        {
            var entity = new JournalHeader
            {
                Id = 1,
                CompanyId = 1,
                VoucherTypeId = 1,
                VoucherNo = "JV/2026-27/0001",
                VoucherDate = new DateOnly(2026, 6, 15),
                TotalDr = 1000m,
                TotalCr = 1000m,
                IsReversal = false
            };

            entity.VoucherNo.Should().Be("JV/2026-27/0001");
            entity.TotalDr.Should().Be(1000m);
            entity.TotalCr.Should().Be(1000m);
            entity.IsReversal.Should().BeFalse();
        }

        [Fact]
        public void JournalHeader_Details_ShouldBeAssignable()
        {
            var entity = new JournalHeader
            {
                Details = new List<JournalDetail> { new() { LineNo = 1, GlAccountId = 5200101, DrAmount = 1000m } }
            };

            entity.Details.Should().HaveCount(1);
        }

        [Fact]
        public void JournalHeader_VoucherNo_DefaultsToNull()
        {
            new JournalHeader().VoucherNo.Should().BeNull();
        }

        // US-GL03-04 — backdating audit metadata.
        [Fact]
        public void JournalHeader_DefaultIsBackdated_ShouldBeFalse()
        {
            // IsBackdated is a DB persisted-computed column — C# never assigns it; default must be false.
            new JournalHeader().IsBackdated.Should().BeFalse();
        }

        [Fact]
        public void JournalHeader_IsBackdated_SetterIsPrivate()
        {
            // IsBackdated is owned by the DB; the public setter must not exist (private setter only).
            var prop = typeof(JournalHeader).GetProperty(nameof(JournalHeader.IsBackdated));
            prop.Should().NotBeNull();
            prop!.GetSetMethod(nonPublic: false).Should().BeNull();
        }

        [Fact]
        public void JournalHeader_DefaultBackdateReason_ShouldBeNull()
        {
            new JournalHeader().BackdateReason.Should().BeNull();
        }

        [Fact]
        public void JournalHeader_BackdateAck_FieldsAssignable()
        {
            var ackAt = DateTimeOffset.UtcNow;
            var entity = new JournalHeader
            {
                BackdateReason = "Bank charge accrual from prior month",
                BackdateAcknowledgedBy = 42,
                BackdateAcknowledgedAt = ackAt
            };

            entity.BackdateReason.Should().Be("Bank charge accrual from prior month");
            entity.BackdateAcknowledgedBy.Should().Be(42);
            entity.BackdateAcknowledgedAt.Should().Be(ackAt);
        }
    }
}
