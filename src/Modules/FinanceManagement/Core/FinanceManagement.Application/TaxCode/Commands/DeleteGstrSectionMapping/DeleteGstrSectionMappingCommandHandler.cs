using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.DeleteGstrSectionMapping
{
    public class DeleteGstrSectionMappingCommandHandler : IRequestHandler<DeleteGstrSectionMappingCommand, bool>
    {
        private readonly ITaxCodeCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteGstrSectionMappingCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteGstrSectionMappingCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteGstrMappingAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "GSTR_SECTION_MAPPING_DELETE",
                actionName: request.Id.ToString(),
                details: $"GSTR section mapping with Id {request.Id} soft deleted.",
                module: "GstrSectionMapping"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
