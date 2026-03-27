using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.ProcessMaster.Commands.CreateProcessMaster
{
    public class CreateProcessMasterCommandHandler : IRequestHandler<CreateProcessMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IProcessMasterCommandRepository _commandRepository;
        private readonly IProcessMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateProcessMasterCommandHandler(
            IProcessMasterCommandRepository commandRepository,
            IProcessMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateProcessMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ProcessMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "PROCESSMASTER_CREATE",
                actionName: request.ProcessName ?? string.Empty,
                details: $"Process Master '{request.ProcessName}' created successfully with Id {newId}.",
                module: "ProcessMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Process Master created successfully.",
                Data = newId
            };
        }
    }
}
