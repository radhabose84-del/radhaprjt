using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnType.Commands.CreateYarnType
{
    public class CreateYarnTypeCommandHandler : IRequestHandler<CreateYarnTypeCommand, ApiResponseDTO<int>>
    {
        private readonly IYarnTypeCommandRepository _commandRepository;
        private readonly IYarnTypeQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateYarnTypeCommandHandler(
            IYarnTypeCommandRepository commandRepository,
            IYarnTypeQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateYarnTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.YarnType>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "YARNTYPE_CREATE",
                actionName: request.YarnTypeCode ?? string.Empty,
                details: $"Yarn Type '{request.YarnTypeCode}' created successfully with Id {newId}.",
                module: "YarnType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Yarn Type created successfully.",
                Data = newId
            };
        }
    }
}
