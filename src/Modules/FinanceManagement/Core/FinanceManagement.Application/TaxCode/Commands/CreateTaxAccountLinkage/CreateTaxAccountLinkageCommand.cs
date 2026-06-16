using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateTaxAccountLinkage
{
    public class CreateTaxAccountLinkageCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int CompanyId { get; set; }
        public int TaxCodeId { get; set; }
        public int GlAccountId { get; set; }
        public DateOnly EffectiveFrom { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
