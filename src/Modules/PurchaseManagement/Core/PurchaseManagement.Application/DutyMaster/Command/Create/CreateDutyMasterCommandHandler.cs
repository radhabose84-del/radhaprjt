using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using PurchaseManagement.Application.Common.Interfaces.IDutyMaster;
using PurchaseManagement.Application.DutyMaster.Command.Create;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.Purchase.DutyMaster.Create
{
    public class CreateDutyMasterCommandHandler(
        IDutyMasterCommandRepository write,
        IMapper mapper,
        IMediator mediator,
        IDocumentSequenceLookup documentSequenceLookup,
        IIPAddressService ipAddressService) : IRequestHandler<CreateDutyMasterCommand, int>
    {
        public async Task<int> Handle(CreateDutyMasterCommand r, CancellationToken ct)
        {
            var entity = mapper.Map<Domain.Entities.DutyMaster>(r.Model);

            // Generate DutyCode from DocumentSequence
            var unitId = ipAddressService.GetUnitId() ?? 0;
            var transactionTypeId = await documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeDutyMaster, MiscEnumEntity.ModulePurchase, unitId)
                ?? throw new InvalidOperationException("No transaction type configured for Duty Master.");
            var sequences = await documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
            entity.DutyCode = sequences.Count > 0
                ? sequences[^1]
                : throw new InvalidOperationException("No document sequence configured for Duty Master.");

            var id = await write.CreateAsync(entity, transactionTypeId, ct);

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
