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
                TemplateCode = "TNC001",
                TemplateName = "Test Template",
                TemplateTypeId = 1,
                TermsHtml = "<p>Terms</p>",
                ApprovalFlag = true
            };

            entity.Id.Should().Be(1);
            entity.TemplateCode.Should().Be("TNC001");
            entity.TemplateName.Should().Be("Test Template");
            entity.TemplateTypeId.Should().Be(1);
            entity.TermsHtml.Should().Be("<p>Terms</p>");
            entity.ApprovalFlag.Should().BeTrue();
        }

        [Fact]
        public void TnCTemplateMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new TnCTemplateMaster
            {
                TemplateCode = null,
                ApprovalFlag = null,
                Applicabilities = null
            };

            entity.TemplateCode.Should().BeNull();
            entity.ApprovalFlag.Should().BeNull();
            entity.Applicabilities.Should().BeNull();
        }
    }
}
