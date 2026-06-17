using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionAccountLinkage
{
    public class CreateGstrSectionAccountLinkageCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int SectionMasterId { get; set; }
        public string? AccountRangeFrom { get; set; }
        public string? AccountRangeTo { get; set; }
        public decimal? DerivedValue { get; set; }
        public decimal ExpectedValue { get; set; }
        public decimal TolerancePercent { get; set; } = 1m;

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
