using MediatR;
using SalesManagement.Application.MarketingOfficer.Dto;

namespace SalesManagement.Application.MarketingOfficer.Queries.GetEmployeeLookup
{
    public class GetEmployeeLookupQuery : IRequest<List<EmployeeLookupDto>>
    {
        public string? OldUnitId { get; set; }
        public string? EmpNo { get; set; }
    }
}
