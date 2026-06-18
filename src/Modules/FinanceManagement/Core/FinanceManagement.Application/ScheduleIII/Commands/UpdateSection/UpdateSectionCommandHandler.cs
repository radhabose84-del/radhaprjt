using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateSection
{
    public class UpdateSectionCommandHandler : IRequestHandler<UpdateSectionCommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSectionCommandHandler(IScheduleIIICommandRepository commandRepository, IMediator mediator, IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ScheduleIIISection>(request);
            var result = await _commandRepository.UpdateSectionAsync(entity);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Update", actionCode: "S3_SECTION_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Schedule III section with Id {request.Id} updated successfully.",
                module: "ScheduleIIISection"), cancellationToken);

            return new ApiResponseDTO<int> { IsSuccess = true, Message = "Section updated successfully.", Data = result };
        }
    }
}
