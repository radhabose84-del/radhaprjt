using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PaymentTermInstallmentEntityTests
    {
        [Fact]
        public void PaymentTermInstallment_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PaymentTermInstallment();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PaymentTermInstallment_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PaymentTermInstallment();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PaymentTermInstallment_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PaymentTermInstallment)).Should().BeTrue();
        }

        [Fact]
        public void PaymentTermInstallment_Properties_ShouldBeAssignable()
        {
            var entity = new PaymentTermInstallment
            {
                Id = 1,
                PaymentTermId = 5,
                SeqNo = 1,
                Percent = 30m,
                DueDays = 15
            };

            entity.Id.Should().Be(1);
            entity.PaymentTermId.Should().Be(5);
            entity.SeqNo.Should().Be(1);
            entity.Percent.Should().Be(30m);
            entity.DueDays.Should().Be(15);
        }
    }
}
