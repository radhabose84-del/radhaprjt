using PurchaseManagement.Application.TnCTemplateMaster.Command;
using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Command.CreateTnCTemplateMasterCommand
{
    public class CreateTnCTemplateMasterCommand  : IRequest<int>
    {
       // public string? TemplateCode { get; set; }
        public string TemplateName { get; set; } = null!;
        public int ModuleId { get; set; }
        public string TermsHtml { get; set; } = null!;

        public List<TncApplicabilityRequestDto>? Applicabilities { get; set; }
        
       
    }
}