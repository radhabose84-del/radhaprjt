using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.MarketingOfficer.Commands.UpdateMarketingOfficer
{
    public class UpdateMarketingOfficerCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; } = null!;
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string Unit { get; set; } = null!;
        public string Department { get; set; } = null!;
        public string Designation { get; set; } = null!;
        public int SalesOfficeId { get; set; }
        public int IsActive { get; set; }
        public List<UpdateOfficerSalesGroupDto> SalesGroups { get; set; } = new();
    }
}
