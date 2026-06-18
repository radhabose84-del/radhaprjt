using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.Get03BDropdownPreview
{
    // CompanyId + DivisionId are resolved server-side from the token (IIPAddressService).
    public class Get03BDropdownPreviewQuery : IRequest<Preview03BDto>
    {
    }
}
