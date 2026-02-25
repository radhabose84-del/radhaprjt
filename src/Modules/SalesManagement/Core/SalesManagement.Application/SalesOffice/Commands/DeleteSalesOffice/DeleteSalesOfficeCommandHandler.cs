using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOffice.Commands.DeleteSalesOffice
{
    public sealed class DeleteSalesOfficeCommandHandler : IRequestHandler<DeleteSalesOfficeCommand, bool>
    {
        private readonly ISalesOfficeCommandRepository _commandRepo;
        private readonly ISalesOfficeQueryRepository _queryRepo;
        private readonly IMediator _mediator;

        public DeleteSalesOfficeCommandHandler(
            ISalesOfficeCommandRepository commandRepo,
            ISalesOfficeQueryRepository queryRepo,
            IMediator mediator)
        {
            _commandRepo = commandRepo;
            _queryRepo = queryRepo;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesOfficeCommand request, CancellationToken ct)
        {
            var before = await _queryRepo.GetByIdAsync(request.Id)
                         ?? throw new ExceptionRules("Sales Office not found.");

            var ok = await _commandRepo.SoftDeleteAsync(request.Id, ct);
            if (!ok) throw new ExceptionRules("Failed to delete Sales Office.");

            var ev = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALES_OFFICE_DELETE",
                actionName: before.SalesOfficeName,
                details: $"Sales Office '{before.SalesOfficeName}' soft-deleted.",
                module: "SalesOffice"
            );
            await _mediator.Publish(ev, ct);

            return true;
        }
    }
}
