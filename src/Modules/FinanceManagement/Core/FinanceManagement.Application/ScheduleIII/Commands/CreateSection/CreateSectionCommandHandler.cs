using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateSection
{
    public class CreateSectionCommandHandler : IRequestHandler<CreateSectionCommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateSectionCommandHandler(IScheduleIIICommandRepository commandRepository, IMediator mediator, IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ScheduleIIISection>(request);
            var newId = await _commandRepository.CreateSectionAsync(entity);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Create", actionCode: "S3_SECTION_CREATE",
                actionName: request.SectionName ?? string.Empty,
                details: $"Schedule III section '{request.SectionName}' created (Id {newId}).",
                module: "ScheduleIIISection"), cancellationToken);

            return new ApiResponseDTO<int> { IsSuccess = true, Message = "Section created successfully.", Data = newId };
        }
    }
}
