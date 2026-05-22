using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.TnCTemplateMaster.Command.CreateTnCTemplateMasterCommand
{
    public class CreateTnCTemplateMasterCommand  : IRequest<int>, IRequirePermission
    {
       // public string? TemplateCode { get; set; }
        public string TemplateName { get; set; } = null!;
        public int TemplateTypeId { get; set; }
        public string TermsHtml { get; set; } = null!;
        public bool? ApprovalFlag { get; set; }
        
        public List<TncApplicabilityDto>? Applicabilities { get; set; } 
        
       
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
