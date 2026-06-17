using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.ActivateTaxAccountLinkage
{
    public sealed record ActivateTaxAccountLinkageCommand(int Id) : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
