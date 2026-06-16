using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.DeleteTaxAccountLinkage
{
    public class DeleteTaxAccountLinkageCommandHandler : IRequestHandler<DeleteTaxAccountLinkageCommand, bool>
    {
        private readonly ITaxCodeCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteTaxAccountLinkageCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteTaxAccountLinkageCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteLinkageAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "TAX_ACCOUNT_LINKAGE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Tax-account linkage Id {request.Id} soft deleted.",
                module: "TaxAccountLinkage"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
