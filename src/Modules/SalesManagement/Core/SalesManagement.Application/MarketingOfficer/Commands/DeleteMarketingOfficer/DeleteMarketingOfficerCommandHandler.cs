using MediatR;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MarketingOfficer.Commands.DeleteMarketingOfficer
{
    public sealed class DeleteMarketingOfficerCommandHandler : IRequestHandler<DeleteMarketingOfficerCommand, bool>
    {
        private readonly IMarketingOfficerCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteMarketingOfficerCommandHandler(
            IMarketingOfficerCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteMarketingOfficerCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "MARKETING_OFFICER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Marketing Officer with Id {request.Id} soft deleted.",
                module: "MarketingOfficer"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
