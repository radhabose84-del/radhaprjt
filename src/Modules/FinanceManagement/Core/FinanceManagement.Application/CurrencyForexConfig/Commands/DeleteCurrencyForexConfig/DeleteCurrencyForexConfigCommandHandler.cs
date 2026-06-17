using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CurrencyForexConfig.Commands.DeleteCurrencyForexConfig
{
    public class DeleteCurrencyForexConfigCommandHandler : IRequestHandler<DeleteCurrencyForexConfigCommand, bool>
    {
        private readonly ICurrencyForexConfigCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteCurrencyForexConfigCommandHandler(
            ICurrencyForexConfigCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteCurrencyForexConfigCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "CURRENCY_FOREX_CONFIG_DELETE",
                actionName: request.Id.ToString(),
                details: $"Currency Forex Config with Id {request.Id} soft deleted.",
                module: "CurrencyForexConfig"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
