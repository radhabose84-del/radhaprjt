using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.DeleteTaxCodeMaster
{
    public class DeleteTaxCodeMasterCommandHandler : IRequestHandler<DeleteTaxCodeMasterCommand, bool>
    {
        private readonly ITaxCodeCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteTaxCodeMasterCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteTaxCodeMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteTaxCodeAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "TAX_CODE_MASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Tax Code with Id {request.Id} soft deleted.",
                module: "TaxCodeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
