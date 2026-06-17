using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionMaster
{
    public class UpdateGstrSectionMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        // ReportTypeId and SectionCode are immutable — excluded.
        public string? SectionName { get; set; }
        public int IsActive { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
