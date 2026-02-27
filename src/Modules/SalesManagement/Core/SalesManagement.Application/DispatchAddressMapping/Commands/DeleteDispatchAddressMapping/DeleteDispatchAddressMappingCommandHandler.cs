using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAddressMapping.Commands.DeleteDispatchAddressMapping
{
    public class DeleteDispatchAddressMappingCommandHandler : IRequestHandler<DeleteDispatchAddressMappingCommand, bool>
    {
        private readonly IDispatchAddressMappingCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteDispatchAddressMappingCommandHandler(
            IDispatchAddressMappingCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteDispatchAddressMappingCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Dispatch Address Mapping not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "DISPATCH_ADDRESS_MAPPING_DELETE",
                actionName: request.Id.ToString(),
                details: $"Dispatch Address Mapping with Id {request.Id} soft deleted.",
                module: "DispatchAddressMapping"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
