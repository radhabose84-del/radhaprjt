using PurchaseManagement.Application.TnCTemplateMaster.Command;
using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Command.UpdateTnCTemplateMasterCommand
{
    public class UpdateTnCTemplateMasterCommand   : IRequest<bool>
    {
        public int Id { get; set; }
        public string? TemplateCode { get; set; }
        public string TemplateName { get; set; } = null!;
        public int ModuleId { get; set; }
        public string TermsHtml { get; set; } = null!;
        public byte IsActive { get; set; }
        public List<TncApplicabilityRequestDto>? Applicabilities { get; set; }
    }
}