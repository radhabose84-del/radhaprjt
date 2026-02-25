using Contracts.Dtos.Lookups.Users;
using MediatR;

namespace InventoryManagement.Application.Reports.GetUnitsByDivision
{
    public class GetUnitsByDivisionQuery : IRequest<List<DivisionUnitLookupDto>>
    {
        public int CompanyId { get; set; }
        public int DivisionId { get; set; }
    }
}
