using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PaymentTermMasterEntityTests
    {
        [Fact]
        public void PaymentTermMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PaymentTermMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PaymentTermMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PaymentTermMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PaymentTermMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PaymentTermMaster)).Should().BeTrue();
        }

        [Fact]
        public void PaymentTermMaster_Properties_ShouldBeAssignable()
        {
            var entity = new PaymentTermMaster
            {
                Id = 1,
                Code = "PT001",
                Description = "Net 30",
                BaselineTypeId = 1,
                CreditDays = 30,
                AdditionalValue = 0m,
                ApplicableForPortal = true
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("PT001");
            entity.Description.Should().Be("Net 30");
            entity.BaselineTypeId.Should().Be(1);
            entity.CreditDays.Should().Be(30);
            entity.ApplicableForPortal.Should().BeTrue();
        }

        [Fact]
        public void PaymentTermMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PaymentTermMaster
            {
                AdvancePercent = null,
                DiscountPercent = null,
                DiscountDays = null,
                GraceDays = null
            };

            entity.AdvancePercent.Should().BeNull();
            entity.DiscountPercent.Should().BeNull();
            entity.DiscountDays.Should().BeNull();
        }
    }
}
