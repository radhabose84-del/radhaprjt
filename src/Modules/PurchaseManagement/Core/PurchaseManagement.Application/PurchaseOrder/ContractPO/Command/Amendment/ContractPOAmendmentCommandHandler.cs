using System.Text.Json;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces;
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
    private readonly ITimeZoneService _tz;
    private readonly IOutboxEventPublisher _outboxEventPublisher;
    private readonly IMiscMasterQueryRepository _misc;

    public ContractPOAmendmentCommandHandler(
        IContractPOCommandRepository commandRepo,
        IContractPOMasterQueryRepository masterQueryRepo,
        IContractPOQueryRepository releaseQueryRepo,
        IMediator mediator,
        IDocumentSequenceLookup documentSequenceLookup,
        IIPAddressService ipAddressService,
        ITimeZoneService tz,
        IOutboxEventPublisher outboxEventPublisher,
        IMiscMasterQueryRepository misc)
    {
        _commandRepo = commandRepo;
        _masterQueryRepo = masterQueryRepo;
        _releaseQueryRepo = releaseQueryRepo;
        _mediator = mediator;
        _documentSequenceLookup = documentSequenceLookup;
        _ipAddressService = ipAddressService;
        _tz = tz;
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

        // GRN check — block amendment if GRN exists
        if (await _releaseQueryRepo.HasAnyGrnAsync(r.Id, ct))
            throw new InvalidOperationException("GRN exists for this PO. Amendment is not allowed.");

        // Timezone
        var tzId = _tz.GetSystemTimeZone();
        if (string.IsNullOrWhiteSpace(tzId) || tzId.Equals("India Standard Time", StringComparison.OrdinalIgnoreCase))
            tzId = "Asia/Kolkata";
        TimeZoneInfo tzi; try { tzi = TimeZoneInfo.FindSystemTimeZoneById(tzId); } catch { tzi = TimeZoneInfo.Local; }
        var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzi);

        // Compute next PONumber & revision
        var newRevisionNo = existing.RevisionNo + 1;
        r.RevisionNo = newRevisionNo;
        r.AmendmentReason = r.AmendmentReason?.Trim();

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

        var unitId = _ipAddressService.GetUnitId()
            ?? throw new ExceptionRules("UnitId is not available for the current user.");

        // Build revision-based PONumber from existing PONumber
        var poNumber = BuildNextRevisionCode(existing.PONumber!, newRevisionNo);

        // Normalize document filenames
        if (r.Documents is { Count: > 0 })
        {
            var baseDir = "PoDocument";
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDir);
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            foreach (var doc in r.Documents.Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName)))
            {
                var oldPath = Path.Combine(uploadDir, doc.FileName!);
                if (File.Exists(oldPath))
                {
                    var finalName = $"{poNumber}_{doc.DocumentId}{Path.GetExtension(oldPath)}";
                    var newPath = Path.Combine(uploadDir, finalName);
                    if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Move(oldPath, newPath, overwrite: true);
                        doc.FileName = finalName;
                    }
                }

                if (doc.UploadedDate == default)
                    doc.UploadedDate = now;
            }
        }

        // Determine transaction type based on PO category (Emergency overrides default)
        var poCategory = await _misc.GetByIdAsync(r.POCategoryId);
        var isEmergency = string.Equals(poCategory?.Description, MiscEnumEntity.EmergencyPO, StringComparison.OrdinalIgnoreCase);
        var transactionTypeName = isEmergency
            ? MiscEnumEntity.TransactionTypeEPO
            : MiscEnumEntity.TransactionTypeCPO;

        var transactionTypeId = await _documentSequenceLookup
            .GetTransactionTypeIdAsync(transactionTypeName, MiscEnumEntity.ModulePurchase, unitId)
            ?? throw new ExceptionRules("No transaction type configured for Contract PO.");

        // Resolve Pending status from MiscMaster
        var pendingStatus = await _misc.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
        var pendingStatusId = pendingStatus.Id;

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
            RevisionNo = newRevisionNo,
            BudgetGroupId = r.BudgetGroupId,
            BudgetMonthId = r.BudgetMonthId,
            BudgetRequestById = r.BudgetRequestById,
            ProjectId = r.ProjectId,
            WBSId = r.WBSId,
            FinancialYearId = r.FinancialYearId
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
        // ATOMIC TRANSACTION: Amend PO + Outbox Events
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

    private static string BuildNextRevisionCode(string oldCode, int nextRev)
    {
        var idx = oldCode.LastIndexOf("-R", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0 && idx + 2 < oldCode.Length && int.TryParse(oldCode[(idx + 2)..], out _))
            return oldCode[..idx] + $"-R{nextRev}";
        return $"{oldCode}-R{nextRev}";
    }
}
