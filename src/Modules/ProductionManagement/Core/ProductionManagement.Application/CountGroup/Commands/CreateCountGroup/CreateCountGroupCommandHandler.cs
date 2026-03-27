using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CountGroup.Commands.CreateCountGroup
{
    public class CreateCountGroupCommandHandler : IRequestHandler<CreateCountGroupCommand, ApiResponseDTO<int>>
    {
        private readonly ICountGroupCommandRepository _commandRepository;
        private readonly ICountGroupQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateCountGroupCommandHandler(
            ICountGroupCommandRepository commandRepository,
            ICountGroupQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateCountGroupCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.CountGroup>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "COUNTGROUP_CREATE",
                actionName: request.CountGroupCode ?? string.Empty,
                details: $"Count Group '{request.CountGroupCode}' created successfully with Id {newId}.",
                module: "CountGroup"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Count Group created successfully.",
                Data = newId
            };
        }
    }
}
