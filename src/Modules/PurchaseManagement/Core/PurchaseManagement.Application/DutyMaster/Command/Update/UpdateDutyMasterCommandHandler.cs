using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IDutyMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.Purchase.DutyMaster.Command.Update;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace Core.Application.Purchase.DutyMaster.Update
{
    public class UpdateDutyMasterCommandHandler(
        IDutyMasterQueryRepository read,
        IDutyMasterCommandRepository write,
        IMapper mapper,IMediator mediator) : IRequestHandler<UpdateDutyMasterCommand,bool>
    {
        public async Task<bool> Handle(UpdateDutyMasterCommand r, CancellationToken ct)
        {
            if (r.Model is null || r.Model.Id <= 0) return false;

            var existing = await read.GetByIdAsync(r.Model.Id, ct);
            if (existing is null) return false;

            mapper.Map(r.Model, existing);
            await write.UpdateAsync(existing, ct);

            await mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: existing.DutyCode,
                actionName: "Duty Master",
                details: $"Duty Master updated, Id={existing.Id}.",
                module: "DutyMaster"
            ), ct);

            return true;   
        }
    }
}
