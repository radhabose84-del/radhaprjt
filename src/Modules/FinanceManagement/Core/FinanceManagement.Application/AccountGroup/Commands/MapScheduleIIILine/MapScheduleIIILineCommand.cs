using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Commands.MapScheduleIIILine
{
    // FR-003: map an Account Group to a Schedule III statutory line (or unmap by passing null).
    public class MapScheduleIIILineCommand : IRequest<ApiResponseDTO<int>>
    {
        public int AccountGroupId { get; set; }

        // NULL = remove the mapping.
        public int? ScheduleIIILineItemId { get; set; }
    }
}
