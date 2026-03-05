using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoTypeMaster.Commands.CreateStoTypeMaster
{
    public class CreateStoTypeMasterCommandHandler : IRequestHandler<CreateStoTypeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IStoTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateStoTypeMasterCommandHandler(
            IStoTypeMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateStoTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.StoTypeMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "STO_TYPE_CREATE",
                actionName: request.StoTypeCode ?? string.Empty,
                details: $"STO Type Master '{request.StoTypeCode}' created successfully with Id {newId}.",
                module: "StoTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "STO Type Master created successfully.",
                Data = newId
            };
        }
    }
}
