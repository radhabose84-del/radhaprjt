using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.DeleteGstrSectionAccountLinkage
{
    public class DeleteGstrSectionAccountLinkageCommandHandler : IRequestHandler<DeleteGstrSectionAccountLinkageCommand, bool>
    {
        private readonly IGstrSectionCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteGstrSectionAccountLinkageCommandHandler(IGstrSectionCommandRepository commandRepository, IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteGstrSectionAccountLinkageCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _commandRepository.DeleteLinkageAsync(request.Id, cancellationToken);
            if (!deleted)
                throw new ExceptionRules("GSTR section-account mapping not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "GSTR_SECTION_LINKAGE_DELETE",
                actionName: request.Id.ToString(),
                details: $"GSTR section-account mapping with Id {request.Id} deleted.",
                module: "GstrSectionAccountLinkage"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
