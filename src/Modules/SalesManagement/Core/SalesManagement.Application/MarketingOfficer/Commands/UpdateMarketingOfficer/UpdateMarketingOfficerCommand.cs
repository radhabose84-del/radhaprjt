using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.MarketingOfficer.Commands.UpdateMarketingOfficer
{
    public class UpdateMarketingOfficerCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? EmployeeName { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string? Unit { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public int SalesOfficeId { get; set; }
        public int IsActive { get; set; }
        public List<UpdateOfficerSalesGroupDto> SalesGroups { get; set; } = new();
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
