using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class TnCTemplateMasterEntityTests
    {
        [Fact]
        public void TnCTemplateMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new TnCTemplateMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void TnCTemplateMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new TnCTemplateMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void TnCTemplateMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(TnCTemplateMaster)).Should().BeTrue();
        }

        [Fact]
        public void TnCTemplateMaster_Properties_ShouldBeAssignable()
        {
            var entity = new TnCTemplateMaster
            {
                Id = 1,
                TemplateCode = "PO-00001",
                TemplateName = "Test Template",
                ModuleId = 1,
                TermsHtml = "<p>Terms</p>"
            };

            entity.Id.Should().Be(1);
            entity.TemplateCode.Should().Be("PO-00001");
            entity.TemplateName.Should().Be("Test Template");
            entity.ModuleId.Should().Be(1);
            entity.TermsHtml.Should().Be("<p>Terms</p>");
        }

        [Fact]
        public void TnCTemplateMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new TnCTemplateMaster
            {
                TemplateCode = null,
                Applicabilities = null
            };

            entity.TemplateCode.Should().BeNull();
            entity.Applicabilities.Should().BeNull();
        }
    }
}
