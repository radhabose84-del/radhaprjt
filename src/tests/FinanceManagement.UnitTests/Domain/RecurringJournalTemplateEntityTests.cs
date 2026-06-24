using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class RecurringJournalTemplateHeaderEntityTests
    {
        [Fact]
        public void Header_DefaultIsActive_ShouldBeActive()
        {
            new RecurringJournalTemplateHeader().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void Header_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new RecurringJournalTemplateHeader().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Header_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RecurringJournalTemplateHeader)).Should().BeTrue();
        }

        [Fact]
        public void Header_Properties_And_Lines_ShouldBeAssignable()
        {
            var entity = new RecurringJournalTemplateHeader
            {
                TemplateName = "Monthly Rent",
                VoucherTypeId = 1,
                FrequencyId = 141,
                StartDate = new DateOnly(2026, 4, 1),
                AutoPost = true,
                AmountAdjustmentRuleId = 151,
                LowRisk = true,
                Lines = new List<RecurringJournalTemplateDetail> { new() { LineNo = 1, GlAccountId = 5400101 } }
            };

            entity.TemplateName.Should().Be("Monthly Rent");
            entity.AutoPost.Should().BeTrue();
            entity.LowRisk.Should().BeTrue();
            entity.Lines.Should().HaveCount(1);
        }
    }

    public class RecurringJournalTemplateDetailEntityTests
    {
        [Fact]
        public void Detail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RecurringJournalTemplateDetail)).Should().BeTrue();
        }

        [Fact]
        public void Detail_Properties_ShouldBeAssignable()
        {
            var entity = new RecurringJournalTemplateDetail
            {
                TemplateId = 10,
                LineNo = 1,
                GlAccountId = 5400101,
                DrAmount = 150000m,
                AmountFormula = "PREV_QTR * 1.05",
                CostCentreId = 1,
                ProfitCentreId = 1
            };

            entity.TemplateId.Should().Be(10);
            entity.GlAccountId.Should().Be(5400101);
            entity.DrAmount.Should().Be(150000m);
            entity.AmountFormula.Should().Be("PREV_QTR * 1.05");
        }
    }
}
