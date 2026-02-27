using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAddressMapping.Commands.CreateDispatchAddressMapping
{
    public class CreateDispatchAddressMappingCommandHandler : IRequestHandler<CreateDispatchAddressMappingCommand, ApiResponseDTO<int>>
    {
        private readonly IDispatchAddressMappingCommandRepository _commandRepository;
        private readonly IDispatchAddressMappingQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateDispatchAddressMappingCommandHandler(
            IDispatchAddressMappingCommandRepository commandRepository,
            IDispatchAddressMappingQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateDispatchAddressMappingCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.DispatchAddressMapping>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "DISPATCH_ADDRESS_MAPPING_CREATE",
                actionName: request.PartyId.ToString(),
                details: $"Dispatch Address Mapping for PartyId {request.PartyId} created successfully with Id {newId}.",
                module: "DispatchAddressMapping"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Dispatch Address Mapping created successfully.",
                Data = newId
            };
        }
    }
}
