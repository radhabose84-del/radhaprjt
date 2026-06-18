using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetStructure
{
    // CompanyId + DivisionId are resolved server-side from the token (IIPAddressService), not the client.
    public class GetStructureQuery : IRequest<ScheduleIIIHeaderDto?>
    {
    }
}
