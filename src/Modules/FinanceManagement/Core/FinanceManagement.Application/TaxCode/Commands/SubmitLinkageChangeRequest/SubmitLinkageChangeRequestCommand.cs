using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.SubmitLinkageChangeRequest
{
    public class SubmitLinkageChangeRequestCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int GlAccountId { get; set; }
        public int NewTaxCodeId { get; set; }
        public int? NewControlAccountId { get; set; }
        public string? Reason { get; set; }
        public DateOnly EffectiveFrom { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
