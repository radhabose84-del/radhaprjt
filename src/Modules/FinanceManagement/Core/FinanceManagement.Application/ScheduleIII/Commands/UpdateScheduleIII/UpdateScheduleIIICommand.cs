using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateScheduleIII
{
    // One call updates the whole structure tree (replace-children). Payload nested under "structure";
    // structure.id = the structure to update.
    public class UpdateScheduleIIICommand : IRequest<ApiResponseDTO<int>>
    {
        public ScheduleIIIInput Structure { get; set; } = new();
    }
}
