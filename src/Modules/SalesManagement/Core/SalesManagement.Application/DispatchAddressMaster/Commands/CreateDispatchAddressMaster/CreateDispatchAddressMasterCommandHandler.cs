using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAddressMaster.Commands.CreateDispatchAddressMaster
{
    public class CreateDispatchAddressMasterCommandHandler : IRequestHandler<CreateDispatchAddressMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IDispatchAddressMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateDispatchAddressMasterCommandHandler(
            IDispatchAddressMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateDispatchAddressMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.DispatchAddressMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "DISPATCH_ADDRESS_CREATE",
                actionName: request.DispatchAddressName,
                details: $"Dispatch Address Master '{request.DispatchAddressName}' created successfully with Id {newId}.",
                module: "DispatchAddressMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Dispatch Address Master created successfully.",
                Data = newId
            };
        }
    }
}
