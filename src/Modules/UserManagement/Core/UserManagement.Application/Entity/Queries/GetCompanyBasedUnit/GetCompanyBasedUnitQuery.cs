using MediatR;

namespace UserManagement.Application.Entity.Queries.GetCompanyBasedUnit
{
    public class GetCompanyBasedUnitQuery : IRequest<List<GetCompanyBasedUnitDto>>
    {
       public List<int>? CompanyIds { get; set; } // For multi-select filter
    }
}