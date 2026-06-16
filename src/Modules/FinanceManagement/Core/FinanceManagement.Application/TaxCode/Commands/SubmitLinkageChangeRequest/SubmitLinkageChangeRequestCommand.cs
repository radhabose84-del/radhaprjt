using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.SubmitLinkageChangeRequest
{
    public class SubmitLinkageChangeRequestCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int CompanyId { get; set; }
        public int GlAccountId { get; set; }
        public int? OldTaxCodeId { get; set; }
        public int NewTaxCodeId { get; set; }
        public string? Reason { get; set; }
        public DateOnly EffectiveFrom { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
