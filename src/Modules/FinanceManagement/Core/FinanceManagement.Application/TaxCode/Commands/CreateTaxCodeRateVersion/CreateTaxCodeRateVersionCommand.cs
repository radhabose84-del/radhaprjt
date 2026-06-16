using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeRateVersion
{
    public class CreateTaxCodeRateVersionCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int TaxCodeId { get; set; }
        public decimal RatePercent { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public string? ChangeReason { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
