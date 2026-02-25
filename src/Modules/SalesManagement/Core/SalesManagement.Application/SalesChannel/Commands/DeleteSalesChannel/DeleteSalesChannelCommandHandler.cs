using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesChannel.Commands.DeleteSalesChannel
{
    public sealed class DeleteSalesChannelCommandHandler
        : IRequestHandler<DeleteSalesChannelCommand, bool>
    {
        private readonly ISalesChannelCommandRepository _commandRepo;
        private readonly ISalesChannelQueryRepository _queryRepo;
        private readonly IMediator _mediator;

        public DeleteSalesChannelCommandHandler(
            ISalesChannelCommandRepository commandRepo,
            ISalesChannelQueryRepository queryRepo,
            IMediator mediator)
        {
            _commandRepo = commandRepo;
            _queryRepo = queryRepo;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesChannelCommand request, CancellationToken ct)
        {
            var before = await _queryRepo.GetByIdAsync(request.Id)
                         ?? throw new ExceptionRules("Sales Channel not found.");

            var ok = await _commandRepo.SoftDeleteAsync(request.Id, ct);
            if (!ok) throw new ExceptionRules("Failed to delete Sales Channel.");

            var ev = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: before.SalesChannelCode,
                actionName: before.SalesChannelName,
                details: $"Sales Channel '{before.SalesChannelCode} - {before.SalesChannelName}' soft-deleted.",
                module: "SalesChannel"
            );
            await _mediator.Publish(ev, ct);

            return true;
        }
    }
}
