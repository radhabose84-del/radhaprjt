using PurchaseManagement.Application.Common.Interfaces.IDutyMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Delete;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.Purchase.DutyMaster.Delete
{
    public class DeleteDutyMasterCommandHandler(
        IDutyMasterQueryRepository read,
        IDutyMasterCommandRepository write,IMediator mediator) : IRequestHandler<DeleteDutyMasterCommand>
    {
        public async Task Handle(DeleteDutyMasterCommand r, CancellationToken ct)
        {
            var existing = await read.GetByIdAsync(r.Id, ct)
                           ?? throw new KeyNotFoundException("DutyMaster not found");

            await write.SoftDeleteAsync(existing.Id, ct);

            // AUDIT LOG event
            var audit = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: existing.DutyCode,
                actionName: "Duty Master",
                details: $"Duty Master '{existing.DutyCode}' (Tariff {existing.TariffNumber}) was deleted, Id={existing.Id}.",
                module: "DutyMaster"
            );
            await mediator.Publish(audit, ct);
        }
    }
}
