using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionMapping
{
    public class UpdateGstrSectionMappingCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? GstrType { get; set; }
        public string? SectionCode { get; set; }
        public string? SectionName { get; set; }
        public string? AccountRangeFrom { get; set; }
        public string? AccountRangeTo { get; set; }
        public decimal? TolerancePercent { get; set; }
        public int IsActive { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
