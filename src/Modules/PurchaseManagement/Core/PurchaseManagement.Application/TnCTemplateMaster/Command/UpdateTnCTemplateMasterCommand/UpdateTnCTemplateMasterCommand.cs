using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.TnCTemplateMaster.Command.UpdateTnCTemplateMasterCommand
{
    public class UpdateTnCTemplateMasterCommand   : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public string? TemplateCode { get; set; }
        public string TemplateName { get; set; } = null!;
        public int TemplateTypeId { get; set; }
        public string TermsHtml { get; set; } = null!;
        public bool? ApprovalFlag { get; set; }
        public byte IsActive { get; set; }
        public List<TncApplicabilityDto>? Applicabilities { get; set; } 
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
