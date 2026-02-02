using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IDutyMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Command.Create;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.Purchase.DutyMaster.Create
{
    public class CreateDutyMasterCommandHandler(
        IDutyMasterCommandRepository write,
        IDutyMasterQueryRepository _repo,
        IMapper mapper,
        IMediator mediator) : IRequestHandler<CreateDutyMasterCommand, int>
    {
        public async Task<int> Handle(CreateDutyMasterCommand r, CancellationToken ct)
        {
            var entity = mapper.Map<Domain.Entities.DutyMaster>(r.Model);
            entity.DutyCode = await _repo.GenerateDutyCodeAsync(ct);
            var id = await write.CreateAsync(entity, ct);

            // AUDIT LOG event
            var audit = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: entity.DutyCode,
                actionName: "Duty Master",
                details: $"Duty Master '{entity.DutyCode}' (Tariff {entity.TariffNumber}) was created with Id={id}.",
                module: "DutyMaster"
            );
            await mediator.Publish(audit, ct);

            return id;
        }
    }
}
