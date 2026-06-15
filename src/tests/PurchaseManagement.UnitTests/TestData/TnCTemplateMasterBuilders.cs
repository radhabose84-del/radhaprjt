using PurchaseManagement.Application.TnCTemplateMaster.Command;
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
            int moduleId = 1,
            string termsHtml = "<p>Test terms and conditions</p>") =>
            new CreateTnCTemplateMasterCommand
            {
                TemplateName = templateName,
                ModuleId = moduleId,
                TermsHtml = termsHtml,
                Applicabilities = new List<TncApplicabilityRequestDto>
                {
                    new TncApplicabilityRequestDto { TransactionTypeId = 1 }
                }
            };

        public static UpdateTnCTemplateMasterCommand ValidUpdateCommand(
            int id = 1,
            string templateName = "Updated Template",
            int moduleId = 1,
            byte isActive = 1) =>
            new UpdateTnCTemplateMasterCommand
            {
                Id = id,
                TemplateName = templateName,
                ModuleId = moduleId,
                TermsHtml = "<p>Updated terms and conditions</p>",
                IsActive = isActive,
                Applicabilities = new List<TncApplicabilityDto>
                {
                    new TncApplicabilityDto { TransactionTypeId = 1 }
                }
            };

        public static DeleteTnCTemplateMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteTnCTemplateMasterCommand { Id = id };

        public static TncTemplateMasterDto ValidDto(int id = 1) =>
            new TncTemplateMasterDto
            {
                Id = id,
                TemplateCode = "PO-00001",
                TemplateName = "Test Template",
                ModuleId = 1,
                ModuleName = "Purchase",
                TermsHtml = "<p>Test terms</p>",
                IsActive = true,
                CreatedDate = DateTimeOffset.UtcNow,
                Applicabilities = new List<TncApplicabilityDto>
                {
                    new TncApplicabilityDto { TransactionTypeId = 1, TypeName = "Purchase Order", ShortName = "PO" }
                }
            };

        public static List<TnCAutoCompleteDto> ValidAutoCompleteList() =>
            new List<TnCAutoCompleteDto>
            {
                new TnCAutoCompleteDto { Id = 1, Code = "PO-00001", TemplateName = "Test Template" }
            };

        public static PurchaseManagement.Domain.Entities.TnCTemplateMaster ValidEntity(int id = 1) =>
            new PurchaseManagement.Domain.Entities.TnCTemplateMaster
            {
                Id = id,
                TemplateCode = "PO-00001",
                TemplateName = "Test Template",
                ModuleId = 1,
                TermsHtml = "<p>Test terms</p>",
                IsActive = DomainBase.Status.Active,
                IsDeleted = DomainBase.IsDelete.NotDeleted
            };
    }
}
