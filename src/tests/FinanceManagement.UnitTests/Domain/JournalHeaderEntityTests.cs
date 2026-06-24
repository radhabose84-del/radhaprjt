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
    }
}
