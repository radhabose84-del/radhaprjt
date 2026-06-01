using System.Text.Json;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBlanketPO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BlanketPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Create;

public sealed class CreateBlanketPOCommandHandler
    : IRequestHandler<CreateBlanketPOCommand, ApiResponseDTO<int>>
{
    private readonly IBlanketPOCommandRepository _commandRepo;
    private readonly IBlanketPOQueryRepository _releaseQueryRepo;
    private readonly IBlanketMasterQueryRepository _masterQueryRepo;
    private readonly IMediator _mediator;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;
    private readonly IIPAddressService _ipAddressService;
    private readonly ITimeZoneService _tz;
    private readonly IOutboxEventPublisher _outboxEventPublisher;
    private readonly IBudgetAllocationLookup _budgetAllocationLookup;
    private readonly IMiscMasterQueryRepository _misc;

    public CreateBlanketPOCommandHandler(
        IBlanketPOCommandRepository commandRepo,
        IBlanketPOQueryRepository releaseQueryRepo,
        IBlanketMasterQueryRepository masterQueryRepo,
        IMediator mediator,
        IDocumentSequenceLookup documentSequenceLookup,
        IIPAddressService ipAddressService,
        ITimeZoneService tz,
        IOutboxEventPublisher outboxEventPublisher,
        IBudgetAllocationLookup budgetAllocationLookup,
        IMiscMasterQueryRepository misc)
    {
        _commandRepo = commandRepo;
        _releaseQueryRepo = releaseQueryRepo;
        _masterQueryRepo = masterQueryRepo;
        _mediator = mediator;
        _documentSequenceLookup = documentSequenceLookup;
        _ipAddressService = ipAddressService;
        _tz = tz;
        _outboxEventPublisher = outboxEventPublisher;
        _budgetAllocationLookup = budgetAllocationLookup;
        _misc = misc;
    }

    public async Task<ApiResponseDTO<int>> Handle(
        CreateBlanketPOCommand request, CancellationToken ct)
    {
        var r = request.Data;

        // Fetch blanket master to get VendorId, CurrencyId
        var blanket = await _masterQueryRepo.GetByIdAsync(r.BlanketHeaderId, ct)
            ?? throw new ExceptionRules("Blanket Master not found.");

        // Ensure the blanket is approved
        if (!await _masterQueryRepo.IsApprovedAsync(r.BlanketHeaderId, ct))
            throw new ExceptionRules("This Blanket is not yet approved. Cannot create Release PO.");

        // Ensure the blanket is not expired
        if (await _masterQueryRepo.IsExpiredAsync(r.BlanketHeaderId, ct))
            throw new ExceptionRules("This Blanket has expired. Cannot create Release PO.");

        // Validate balance: release quantity must not exceed blanket balance
        var balanceErrors = new List<string>();
        foreach (var item in r.Details)
        {
            var balance = await _releaseQueryRepo.GetBlanketDetailBalanceAsync(item.BlanketDetailId);
            if (item.Quantity > balance)
            {
                balanceErrors.Add(
                    $"Item {item.ItemSno}: Release quantity ({item.Quantity}) exceeds available blanket balance ({balance}) for Blanket Detail Id {item.BlanketDetailId}.");
            }
        }
        if (balanceErrors.Count > 0)
            throw new ExceptionRules(string.Join(" ", balanceErrors));

        // Generate PONumber from DocumentSequence
        var unitId = _ipAddressService.GetUnitId()
            ?? throw new ExceptionRules("UnitId is not available for the current user.");

        // Determine transaction type based on PO category
        var poCategory = await _misc.GetByIdAsync(r.POCategoryId);
        var isEmergency = string.Equals(poCategory?.Description, MiscEnumEntity.EmergencyPO, StringComparison.OrdinalIgnoreCase);
        var transactionTypeName = isEmergency
            ? MiscEnumEntity.TransactionTypeEPO
            : MiscEnumEntity.TransactionTypeBPO;

        var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
            transactionTypeName, MiscEnumEntity.ModulePurchase, unitId)
            ?? throw new ExceptionRules("No transaction type configured for Blanket PO.");

        var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
        var poNumber = sequences.Count > 0
            ? sequences[^1]
            : throw new ExceptionRules("No document sequence configured for Blanket PO.");

        // -------------------------------------------------
        // BUDGET VALIDATION (Optimistic - read-only check)
        // -------------------------------------------------
        if (r.BudgetGroupId.HasValue && r.BudgetGroupId.Value > 0)
        {
            DateOnly? budgetDate = null;
            if (r.PODate != default)
                budgetDate = DateOnly.FromDateTime(r.PODate.DateTime);

            var remaining = await _budgetAllocationLookup.GetRemainingBalanceAsync(
                budgetGroupId: r.BudgetGroupId.Value,
                budgetDate: budgetDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                monthId: r.BudgetMonthId ?? 0,
                requestById: r.BudgetRequestById ?? 0,
                projectId: r.ProjectId,
                wbsId: r.WBSId,
                financialYearId: r.FinancialYearId,
                ct: ct);

            var currentRemaining = remaining?.CurrentRemainingBalance ?? 0m;

            if (r.PurchaseValue > currentRemaining)
            {
                return new ApiResponseDTO<int>
                {
                    IsSuccess = false,
                    Message = "Cannot create Blanket Release PO. Budget amount less than PO value. Budget Balance: " + currentRemaining,
                    Data = 0
                };
            }
        }

        // Resolve Pending status from MiscMaster
        var pendingStatus = await _misc.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
        var pendingStatusId = pendingStatus.Id;

        // Build PurchaseOrderHeader
        var poHeader = new PurchaseOrderHeader
        {
            UnitId = unitId,
            PONumber = poNumber,
            PODate = r.PODate,
            POCategoryId = r.POCategoryId,
            POMethodId = r.POMethodId,
            CurrencyId = blanket.CurrencyId,
            VendorId = blanket.VendorId,
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
            CostCenterId = r.CostCenterId,
            BudgetGroupId = r.BudgetGroupId,
            BudgetMonthId = r.BudgetMonthId,
            BudgetRequestById = r.BudgetRequestById,
            ProjectId = r.ProjectId,
            WBSId = r.WBSId,
            FinancialYearId = r.FinancialYearId
        };

        // Build PurchaseBlanketHeader
        var blanketHeader = new PurchaseBlanketHeader
        {
            BlanketHeaderId = r.BlanketHeaderId,
            IsPartialReceiptAllowed = r.IsPartialReceiptAllowed,
            IncotermsId = r.IncotermsId,
            ModeOfDispatchId = r.ModeOfDispatchId,
            FreightCharges = r.FreightCharges,
            TermsId = r.TermsId,
            TermDescription = r.TermDescription,
            DeliveryAddress = r.DeliveryAddress,
            BillingAddress = r.BillingAddress
        };

        // Build PaymentTerms
        var paymentTerms = new List<PurchasePaymentTerm>();
        foreach (var t in r.PaymentTerms)
        {
            paymentTerms.Add(new PurchasePaymentTerm
            {
                PaymentTermId = t.PaymentTermId,
                AdvancePercent = t.AdvancePercent,
                CreditDays = t.CreditDays,
                PaymentModelId = t.PaymentModelId,
                InsuranceId = t.InsuranceId,
                InsurancePercent = t.InsurancePercent,
                InsuranceAmount = t.InsuranceAmount,
                AdvanceAmount = t.AdvanceAmount,
                BalancePercent = t.BalancePercent,
                BalanceAmount = t.BalanceAmount
            });
        }

        // Build PurchaseBlanketDetail
        var blanketDetails = new List<PurchaseBlanketDetail>();
        foreach (var item in r.Details)
        {
            blanketDetails.Add(new PurchaseBlanketDetail
            {
                BlanketDetailId = item.BlanketDetailId,
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
        }

        // ════════════════════════════════════════════════════════════════════════
        // ATOMIC TRANSACTION: PO Creation + Outbox Events + DocNo Increment
        // ════════════════════════════════════════════════════════════════════════

        var strategy = _commandRepo.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            var (transaction, dbConn, dbTx) = await _commandRepo.BeginTransactionWithConnectionAsync(ct);
            await using var _ = transaction;

            try
            {
                var newPOId = await _commandRepo.CreateWithoutTransactionAsync(
                    poHeader, blanketHeader, blanketDetails, paymentTerms, ct);

                // ── Budget allocation (same transaction) ─────────────────────────
                if (poHeader.BudgetGroupId.HasValue && poHeader.BudgetGroupId.Value > 0 && poHeader.PurchaseValue > 0)
                {
                    var budgetMonthDate = new DateOnly(poHeader.PODate.Year, poHeader.PODate.Month, 1);
                    var deltaApplied = await _budgetAllocationLookup.ApplyRemainingBalanceDeltaAsync(
                        budgetGroupId: poHeader.BudgetGroupId.Value,
                        budgetDate: budgetMonthDate,
                        monthId: poHeader.BudgetMonthId ?? 0,
                        requestById: poHeader.BudgetRequestById ?? 0,
                        deltaAmount: -poHeader.PurchaseValue,
                        projectId: poHeader.ProjectId,
                        wbsId: poHeader.WBSId,
                        financialYearId: poHeader.FinancialYearId,
                        connection: dbConn,
                        transaction: dbTx,
                        ct: ct);

                    if (!deltaApplied)
                    {
                        await transaction.RollbackAsync(ct);
                        return new ApiResponseDTO<int>
                        {
                            IsSuccess = false,
                            Message = "Budget allocation failed. Blanket Release PO creation rolled back.",
                            Data = 0
                        };
                    }
                }

                // ── Approval workflow (outbox — same transaction) ──────────────────
                var correlationId = Guid.NewGuid();

                var workFlowEntity = await _commandRepo.GetByIdBlanketPOWorkFlowAsync(newPOId);
                var reversePayload = new CreateBlanketPOReverseDto
                {
                    Header = workFlowEntity,
                    Lines = null
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
                    actionDetail: "Create",
                    actionCode: "BLANKET_RELEASE_PO_CREATE",
                    actionName: poNumber,
                    details: $"Blanket Release PO '{poNumber}' created against Blanket '{blanket.BlanketNumber}' with Id {newPOId}.",
                    module: "BlanketPO"
                );
                await _mediator.Publish(ev, ct);

                return new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Message = $"Blanket Release PO '{poNumber}' created successfully.",
                    Data = newPOId
                };
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }
}
