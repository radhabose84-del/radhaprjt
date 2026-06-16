using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionMapping
{
    public class CreateGstrSectionMappingCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int CompanyId { get; set; }
        public string? GstrType { get; set; }            // GSTR1 / GSTR3B
        public string? SectionCode { get; set; }
        public string? SectionName { get; set; }
        public string? AccountRangeFrom { get; set; }
        public string? AccountRangeTo { get; set; }
        public decimal? TolerancePercent { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
