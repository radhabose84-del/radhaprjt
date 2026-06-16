using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateScheduleIII
{
    public class CreateScheduleIIICommandHandler : IRequestHandler<CreateScheduleIIICommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public CreateScheduleIIICommandHandler(IScheduleIIICommandRepository commandRepository, IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateScheduleIIICommand request, CancellationToken cancellationToken)
        {
            var structureId = await _commandRepository.CreateAggregateAsync(request.Structure);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Create", actionCode: "S3_AGGREGATE_CREATE",
                actionName: structureId.ToString(),
                details: $"Schedule III structure created (Id {structureId}) with {request.Structure.Sections.Count} section(s) and {request.Structure.SubTotals.Count} sub-total(s).",
                module: "ScheduleIIIStructure"), cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Schedule III structure created successfully.",
                Data = structureId
            };
        }
    }
}
