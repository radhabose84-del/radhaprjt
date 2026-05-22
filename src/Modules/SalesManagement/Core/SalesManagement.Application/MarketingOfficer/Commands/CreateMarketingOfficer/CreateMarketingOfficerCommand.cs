using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.MarketingOfficer.Commands.CreateMarketingOfficer
{
    public class CreateMarketingOfficerCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string? Unit { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public int SalesOfficeId { get; set; }
        public List<CreateOfficerSalesGroupDto> SalesGroups { get; set; } = new();
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
