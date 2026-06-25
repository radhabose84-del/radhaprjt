using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Commands.DeleteFinancialYearMaster
{
    public class DeleteFinancialYearMasterCommandHandler : IRequestHandler<DeleteFinancialYearMasterCommand, bool>
    {
        private readonly IFinancialYearMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteFinancialYearMasterCommandHandler(
            IFinancialYearMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteFinancialYearMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "FINANCIAL_YEAR_MASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Financial Year with Id {request.Id} soft deleted.",
                module: "FinancialYearMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
