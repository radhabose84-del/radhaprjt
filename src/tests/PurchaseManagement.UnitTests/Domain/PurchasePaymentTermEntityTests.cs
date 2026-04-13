using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PurchasePaymentTermEntityTests
    {
        [Fact]
        public void PurchasePaymentTerm_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PurchasePaymentTerm();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PurchasePaymentTerm_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PurchasePaymentTerm();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PurchasePaymentTerm_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PurchasePaymentTerm)).Should().BeTrue();
        }

        [Fact]
        public void PurchasePaymentTerm_Properties_ShouldBeAssignable()
        {
            var entity = new PurchasePaymentTerm
            {
                Id = 1,
                PurchaseOrderId = 5,
                PaymentTermId = 7,
                AdvancePercent = 30m,
                CreditDays = 45,
                PaymentModelId = 2,
                InsuranceId = 3,
                InsurancePercent = 5,
                InsuranceAmount = 250m,
                AdvanceAmount = 1500m,
                BalancePercent = 70m,
                BalanceAmount = 3500m
            };

            entity.Id.Should().Be(1);
            entity.PurchaseOrderId.Should().Be(5);
            entity.PaymentTermId.Should().Be(7);
            entity.AdvancePercent.Should().Be(30m);
            entity.CreditDays.Should().Be(45);
            entity.AdvanceAmount.Should().Be(1500m);
            entity.BalanceAmount.Should().Be(3500m);
        }

        [Fact]
        public void PurchasePaymentTerm_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PurchasePaymentTerm
            {
                AdvancePercent = null,
                CreditDays = null,
                PaymentModelId = null,
                InsuranceId = null,
                InsurancePercent = null,
                InsuranceAmount = null,
                AdvanceAmount = null,
                BalancePercent = null,
                BalanceAmount = null,
                PurchaseTerm = null,
                MiscPOPaymentTerm = null,
                MiscPOPaymentMode = null
            };

            entity.AdvancePercent.Should().BeNull();
            entity.CreditDays.Should().BeNull();
            entity.PurchaseTerm.Should().BeNull();
            entity.MiscPOPaymentTerm.Should().BeNull();
        }
    }
}
