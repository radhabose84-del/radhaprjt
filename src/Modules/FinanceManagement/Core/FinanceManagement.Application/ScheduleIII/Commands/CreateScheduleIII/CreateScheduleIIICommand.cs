using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateScheduleIII
{
    // One call inserts Structure + Sections + LineItems + SubTotals + Formulas (all five tables).
    // Payload is nested under "structure".
    public class CreateScheduleIIICommand : IRequest<ApiResponseDTO<int>>
    {
        public ScheduleIIIInput Structure { get; set; } = new();
    }
}
