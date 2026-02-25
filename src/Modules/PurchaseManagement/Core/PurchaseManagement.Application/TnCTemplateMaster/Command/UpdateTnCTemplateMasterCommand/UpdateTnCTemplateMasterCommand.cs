using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Command.UpdateTnCTemplateMasterCommand
{
    public class UpdateTnCTemplateMasterCommand   : IRequest<bool>
    {
        public int Id { get; set; }
        public string? TemplateCode { get; set; }
        public string TemplateName { get; set; } = null!;
        public int TemplateTypeId { get; set; }
        public string TermsHtml { get; set; } = null!;
        public bool? ApprovalFlag { get; set; }
        public byte IsActive { get; set; }
        public List<TncApplicabilityDto>? Applicabilities { get; set; } 
    }
}