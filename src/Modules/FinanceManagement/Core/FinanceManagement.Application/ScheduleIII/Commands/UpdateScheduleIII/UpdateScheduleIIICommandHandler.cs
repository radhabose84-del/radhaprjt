using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateScheduleIII
{
    public class UpdateScheduleIIICommandHandler : IRequestHandler<UpdateScheduleIIICommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public UpdateScheduleIIICommandHandler(IScheduleIIICommandRepository commandRepository, IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateScheduleIIICommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.UpdateAggregateAsync(request.Structure);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Update", actionCode: "S3_AGGREGATE_UPDATE",
                actionName: request.Structure.Id.ToString(),
                details: $"Schedule III structure {request.Structure.Id} updated (tree replaced).",
                module: "ScheduleIIIStructure"), cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Schedule III structure updated successfully.",
                Data = result
            };
        }
    }
}
