using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionAccountLinkage
{
    public class UpdateGstrSectionAccountLinkageCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public int SectionMasterId { get; set; }
        public string? AccountRangeFrom { get; set; }
        public string? AccountRangeTo { get; set; }
        public decimal? DerivedValue { get; set; }
        public decimal ExpectedValue { get; set; }
        public decimal TolerancePercent { get; set; } = 1m;
        public int IsActive { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
