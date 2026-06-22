using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class JournalDetailEntityTests
    {
        [Fact]
        public void JournalDetail_DefaultIsActive_ShouldBeActive()
        {
            new JournalDetail().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void JournalDetail_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new JournalDetail().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void JournalDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(JournalDetail)).Should().BeTrue();
        }

        [Fact]
        public void JournalDetail_Properties_ShouldBeAssignable()
        {
            var entity = new JournalDetail
            {
                Id = 1,
                JournalHeaderId = 10,
                LineNo = 1,
                GlAccountId = 5200101,
                DrAmount = 1000m,
                CrAmount = 0m,
                CurrencyId = 1,
                CostCentreId = 1,
                ProfitCentreId = 1,
                ReferenceDocNo = "REF/001"
            };

            entity.JournalHeaderId.Should().Be(10);
            entity.GlAccountId.Should().Be(5200101);
            entity.DrAmount.Should().Be(1000m);
            entity.CostCentreId.Should().Be(1);
            entity.ReferenceDocNo.Should().Be("REF/001");
        }
    }
}
