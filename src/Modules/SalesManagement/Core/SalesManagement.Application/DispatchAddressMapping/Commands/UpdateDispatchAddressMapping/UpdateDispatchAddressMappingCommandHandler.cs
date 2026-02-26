using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAddressMapping.Commands.UpdateDispatchAddressMapping
{
    public class UpdateDispatchAddressMappingCommandHandler : IRequestHandler<UpdateDispatchAddressMappingCommand, ApiResponseDTO<int>>
    {
        private readonly IDispatchAddressMappingCommandRepository _commandRepository;
        private readonly IDispatchAddressMappingQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateDispatchAddressMappingCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateDispatchAddressMappingCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.DispatchAddressMapping>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "DISPATCH_ADDRESS_MAPPING_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Dispatch Address Mapping with Id {request.Id} updated successfully.",
                module: "DispatchAddressMapping"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Dispatch Address Mapping updated successfully.",
                Data = updatedId
            };
        }
    }
}
