using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class TnCTemplateApplicabilityEntityTests
    {
        [Fact]
        public void TnCTemplateApplicability_DefaultIsActive_ShouldBeActive()
        {
            var entity = new TnCTemplateApplicability();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void TnCTemplateApplicability_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new TnCTemplateApplicability();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void TnCTemplateApplicability_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(TnCTemplateApplicability)).Should().BeTrue();
        }

        [Fact]
        public void TnCTemplateApplicability_Properties_ShouldBeAssignable()
        {
            var entity = new TnCTemplateApplicability
            {
                Id = 1,
                TnCTemplateMasterId = 5,
                TransactionTypeId = 7
            };

            entity.Id.Should().Be(1);
            entity.TnCTemplateMasterId.Should().Be(5);
            entity.TransactionTypeId.Should().Be(7);
        }
    }
}
