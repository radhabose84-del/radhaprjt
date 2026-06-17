using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.DeleteGstrSectionMaster
{
    public class DeleteGstrSectionMasterCommandHandler : IRequestHandler<DeleteGstrSectionMasterCommand, bool>
    {
        private readonly IGstrSectionCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteGstrSectionMasterCommandHandler(IGstrSectionCommandRepository commandRepository, IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteGstrSectionMasterCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _commandRepository.DeleteSectionAsync(request.Id, cancellationToken);
            if (!deleted)
                throw new ExceptionRules("GSTR section not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "GSTR_SECTION_MASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"GSTR section with Id {request.Id} deleted.",
                module: "GstrSectionMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
