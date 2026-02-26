using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.MarketingOfficer.Commands.CreateMarketingOfficer
{
    public class CreateMarketingOfficerCommand : IRequest<ApiResponseDTO<int>>
    {
        public string EmployeeNo { get; set; } = null!;
        public string EmployeeName { get; set; } = null!;
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string Unit { get; set; } = null!;
        public string Department { get; set; } = null!;
        public string Designation { get; set; } = null!;
        public int SalesOfficeId { get; set; }
        public List<CreateOfficerSalesGroupDto> SalesGroups { get; set; } = new();
    }
}
