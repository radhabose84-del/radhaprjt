using System.Text.Json;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Create;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Amendment;

public sealed class ContractPOAmendmentCommandHandler
    : IRequestHandler<ContractPOAmendmentCommand, int>
{
    private readonly IContractPOCommandRepository _commandRepo;
    private readonly IContractPOMasterQueryRepository _masterQueryRepo;
    private readonly IContractPOQueryRepository _releaseQueryRepo;
    private readonly IMediator _mediator;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;
    private readonly IIPAddressService _ipAddressService;
    private readonly IOutboxEventPublisher _outboxEventPublisher;
    private readonly IMiscMasterQueryRepository _misc;

    public ContractPOAmendmentCommandHandler(
        IContractPOCommandRepository commandRepo,
        IContractPOMasterQueryRepository masterQueryRepo,
        IContractPOQueryRepository releaseQueryRepo,
        IMediator mediator,
        IDocumentSequenceLookup documentSequenceLookup,
        IIPAddressService ipAddressService,
        IOutboxEventPublisher outboxEventPublisher,
        IMiscMasterQueryRepository misc)
    {
        _commandRepo = commandRepo;
        _masterQueryRepo = masterQueryRepo;
        _releaseQueryRepo = releaseQueryRepo;
        _mediator = mediator;
        _documentSequenceLookup = documentSequenceLookup;
        _ipAddressService = ipAddressService;
        _outboxEventPublisher = outboxEventPublisher;
        _misc = misc;
    }

    public async Task<int> Handle(ContractPOAmendmentCommand request, CancellationToken ct)
    {
        var r = request.Data;

        // Get existing PO to verify and get current revision
        var existing = await _releaseQueryRepo.GetContractPOByIdAsync(r.Id, ct)
            ?? throw new ExceptionRules("Contract Release PO not found.");

        // Fetch contract to get VendorId, CurrencyId
        var contract = await _masterQueryRepo.GetByIdAsync(r.ContractPOHeaderId, ct)
            ?? throw new ExceptionRules("Contract PO not found.");

        // Ensure the contract is approved before allowing release PO amendment
        if (contract.StatusName != MiscEnumEntity.Approved)
            throw new ExceptionRules("This contract is not yet approved. Cannot amend Release PO.");

        // Validate balance: release quantity must not exceed contract balance
        // For amendment, add back old PO's quantities since they will be reversed
        var balanceErrors = new List<string>();
        foreach (var item in r.Details)
        {
            var currentBalance = await _releaseQueryRepo.GetContractDetailBalanceAsync(item.ContractPODetailId);
            // Add back old PO's quantity for the same ContractPODetailId (will be reversed during amendment)
            var oldQtyForThisLine = existing.Details
                .Where(d => d.ContractPODetailId == item.ContractPODetailId)
                .Sum(d => d.Quantity);
            var availableBalance = currentBalance + oldQtyForThisLine;
            if (item.Quantity > availableBalance)
            {
                balanceErrors.Add(
                    $"Item {item.ItemSno}: Release quantity ({item.Quantity}) exceeds available contract balance ({availableBalance}) for Contract Detail Id {item.ContractPODetailId}.");
            }
        }
        if (balanceErrors.Count > 0)
            throw new ExceptionRules(string.Join(" ", balanceErrors));

        // Generate new PONumber
        var unitId = _ipAddressService.GetUnitId()
            ?? throw new ExceptionRules("UnitId is not available for the current user.");

        // Determine transaction type based on PO category (Emergency overrides default)
        var poCategory = await _misc.GetByIdAsync(r.POCategoryId);
        var isEmergency = string.Equals(poCategory?.Description, MiscEnumEntity.EmergencyPO, StringComparison.OrdinalIgnoreCase);
        var transactionTypeName = isEmergency
            ? MiscEnumEntity.TransactionTypeEPO
            : MiscEnumEntity.TransactionTypeCPO;

        var transactionTypeId = await _documentSequenceLookup
            .GetTransactionTypeIdAsync(transactionTypeName, MiscEnumEntity.ModulePurchase, unitId)
            ?? throw new ExceptionRules("No transaction type configured for Contract PO.");

        var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
        var poNumber = sequences.Count > 0
            ? sequences[^1]
            : throw new ExceptionRules("No document sequence configured for Contract PO.");

        // Resolve Pending status from MiscMaster
        var pendingStatus = await _misc.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
        var pendingStatusId = pendingStatus.Id;

        var newRevisionNo = existing.RevisionNo + 1;

        // Build new PurchaseOrderHeader
        var newPoHeader = new PurchaseOrderHeader
        {
            UnitId = unitId,
            PONumber = poNumber,
            PODate = r.PODate,
            POCategoryId = r.POCategoryId,
            POMethodId = r.POMethodId,
            CurrencyId = contract.CurrencyId,
            VendorId = contract.VendorId,
            ItemTotal = r.ItemTotal,
            DiscountTotal = r.DiscountTotal,
            PandFTotal = r.PandFTotal,
            MiscCharges = r.MiscCharges,
            GSTTotal = r.GSTTotal,
            CGSTTotal = r.CGSTTotal,
            SGSTTotal = r.SGSTTotal,
            IGSTTotal = r.IGSTTotal,
            FreightTotal = r.FreightTotal,
            PurchaseValue = r.PurchaseValue,
            StatusId = pendingStatusId,
            RevisionNo = newRevisionNo
        };

        // Build new PurchaseContractHeader
        var newContractHeader = new PurchaseContractHeader
        {
            ContractPOHeaderId = r.ContractPOHeaderId,
            IsPartialReceiptAllowed = r.IsPartialReceiptAllowed,
            IncotermsId = r.IncotermsId,
            ModeOfDispatchId = r.ModeOfDispatchId,
            FreightCharges = r.FreightCharges,
            TermsId = r.TermsId,
            TermDescription = r.TermDescription,
            DeliveryAddress = r.DeliveryAddress,
            BillingAddress = r.BillingAddress
        };

        // Build detail + release history entries
        var newContractDetails = new List<PurchaseContractDetail>();
        var newReleaseHistories = new List<ContractPOReleaseHistory>();

        foreach (var item in r.Details)
        {
            newContractDetails.Add(new PurchaseContractDetail
            {
                ContractPODetailId = item.ContractPODetailId,
                ItemSno = item.ItemSno,
                ItemId = item.ItemId,
                UOMId = item.UOMId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                ItemValue = item.ItemValue,
                DiscountTypeId = item.DiscountTypeId,
                DiscountValue = item.DiscountValue,
                PandFType = item.PandFType,
                PandFCharge = item.PandFCharge,
                OtherCharge = item.OtherCharge,
                GSTPercentage = item.GSTPercentage,
                CGSTPercentage = item.CGSTPercentage,
                SGSTPercentage = item.SGSTPercentage,
                IGSTPercentage = item.IGSTPercentage,
                CGST = item.CGST,
                SGST = item.SGST,
                IGST = item.IGST,
                ScheduleDate = item.ScheduleDate,
                DepartmentId = item.DepartmentId
            });

            newReleaseHistories.Add(new ContractPOReleaseHistory
            {
                ContractPOHeaderId = r.ContractPOHeaderId,
                ContractPODetailId = item.ContractPODetailId,
                ReleaseDate = r.PODate,
                ReleasedQuantity = item.Quantity,
                ReleasedRate = item.UnitPrice,
                ReleasedValue = item.ItemValue
            });
        }

        // ════════════════════════════════════════════════════════════════════════
        // ATOMIC TRANSACTION: Amend PO + Outbox Events + DocNo Increment
        // ════════════════════════════════════════════════════════════════════════

        var strategy = _commandRepo.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            var (transaction, dbConn, dbTx) = await _commandRepo.BeginTransactionWithConnectionAsync(ct);
            await using var _ = transaction;

            try
            {
                var newPOId = await _commandRepo.AmendWithoutTransactionAsync(
                    r.Id, newPoHeader, newContractHeader, newContractDetails, newReleaseHistories, ct);

                // ── Approval workflow (outbox — same transaction) ──────────────────
                var correlationId = Guid.NewGuid();
                var reversePayload = new CreateContractPOReverseDto
                {
                    Header = new ContractPOWorkFlowDto
                    {
                        Id = newPOId,
                        PONumber = poNumber,
                        VendorId = contract.VendorId,
                        StatusId = pendingStatusId,
                        UnitId = unitId
                    },
                    Lines = newContractDetails.Select(_ => new ContractPOWorkFlowDto
                    {
                        Id = newPOId,
                        PONumber = poNumber,
                        VendorId = contract.VendorId,
                        StatusId = pendingStatusId,
                        UnitId = unitId
                    }).ToList()
                };

                var workflowCommand = new CreateApprovalRequestCommand
                {
                    CorrelationId = correlationId,
                    ModuleTypeName = MiscEnumEntity.POLocal,
                    ModuleTransactionId = newPOId,
                    Payload = JsonSerializer.Serialize(reversePayload),
                    TransactionTypeId = transactionTypeId
                };

                await _outboxEventPublisher.ScheduleWithoutSaveAsync(workflowCommand, correlationId, ct);

                await _commandRepo.SaveChangesAsync(ct);

                // Increment DocNo via lookup — same connection + transaction
                await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConn, dbTx);

                await transaction.CommitAsync(ct);

                // Audit (outside transaction — fire-and-forget to MongoDB)
                var ev = new AuditLogsDomainEvent(
                    actionDetail: "Amendment",
                    actionCode: "CONTRACT_RELEASE_PO_AMEND",
                    actionName: poNumber,
                    details: $"Contract Release PO amended. Old PO Id: {r.Id}, New PO '{poNumber}' with Id {newPOId}, Revision {newRevisionNo}.",
                    module: "ContractPO"
                );
                await _mediator.Publish(ev, ct);

                return newPOId;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }
}
