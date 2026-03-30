using PurchaseManagement.Application.TnCTemplateMaster.Command.CreateTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Command.DeleteTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Command.UpdateTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterAutoComplete;
using DomainBase = PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class TnCTemplateMasterBuilders
    {
        public static CreateTnCTemplateMasterCommand ValidCreateCommand(
            string templateName = "Test Template",
            int templateTypeId = 1,
            string termsHtml = "<p>Test terms and conditions</p>",
            bool? approvalFlag = false) =>
            new CreateTnCTemplateMasterCommand
            {
                TemplateName = templateName,
                TemplateTypeId = templateTypeId,
                TermsHtml = termsHtml,
                ApprovalFlag = approvalFlag,
                Applicabilities = new List<TncApplicabilityDto>
                {
                    new TncApplicabilityDto { ApplicabilityId = 1 }
                }
            };

        public static UpdateTnCTemplateMasterCommand ValidUpdateCommand(
            int id = 1,
            string templateName = "Updated Template",
            int templateTypeId = 1,
            byte isActive = 1) =>
            new UpdateTnCTemplateMasterCommand
            {
                Id = id,
                TemplateName = templateName,
                TemplateTypeId = templateTypeId,
                TermsHtml = "<p>Updated terms and conditions</p>",
                ApprovalFlag = false,
                IsActive = isActive,
                Applicabilities = new List<TncApplicabilityDto>
                {
                    new TncApplicabilityDto { ApplicabilityId = 1 }
                }
            };

        public static DeleteTnCTemplateMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteTnCTemplateMasterCommand { Id = id };

        public static TncTemplateMasterDto ValidDto(int id = 1) =>
            new TncTemplateMasterDto
            {
                Id = id,
                TemplateCode = "TNC001",
                TemplateName = "Test Template",
                TemplateTypeId = 1,
                TemplateTypeCode = "PURCHASE",
                TemplateTypeDescription = "Purchase Template",
                TermsHtml = "<p>Test terms</p>",
                ApprovalFlag = false,
                IsActive = true,
                CreatedDate = DateTimeOffset.UtcNow,
                Applicabilities = new List<TncApplicabilityDto>
                {
                    new TncApplicabilityDto { ApplicabilityId = 1 }
                }
            };

        public static List<TnCAutoCompleteDto> ValidAutoCompleteList() =>
            new List<TnCAutoCompleteDto>
            {
                new TnCAutoCompleteDto { Id = 1, Code = "TNC001", TemplateName = "Test Template" }
            };

        public static PurchaseManagement.Domain.Entities.TnCTemplateMaster ValidEntity(int id = 1) =>
            new PurchaseManagement.Domain.Entities.TnCTemplateMaster
            {
                Id = id,
                TemplateCode = "TNC001",
                TemplateName = "Test Template",
                TemplateTypeId = 1,
                TermsHtml = "<p>Test terms</p>",
                ApprovalFlag = false,
                IsActive = DomainBase.Status.Active,
                IsDeleted = DomainBase.IsDelete.NotDeleted
            };
    }
}
