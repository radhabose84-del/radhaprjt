using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionMaster
{
    public class CreateGstrSectionMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int ReportTypeId { get; set; }            // FK -> MiscMaster (GSTR_REPORT)
        public string? SectionCode { get; set; }
        public string? SectionName { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
