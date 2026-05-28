using System.Text.Json;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Create;

public sealed class CreateContractPOCommandHandler
    : IRequestHandler<CreateContractPOCommand, ApiResponseDTO<int>>
{
    private readonly IContractPOCommandRepository _commandRepo;
    private readonly IContractPOQueryRepository _releaseQueryRepo;
    private readonly IContractPOMasterQueryRepository _queryRepo;
    private readonly IMediator _mediator;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;
    private readonly IIPAddressService _ipAddressService;
    private readonly ITimeZoneService _tz;
    private readonly IOutboxEventPublisher _outboxEventPublisher;
    private readonly IBudgetAllocationLookup _budgetAllocationLookup;
    private readonly IMiscMasterQueryRepository _misc;

    public CreateContractPOCommandHandler(
        IContractPOCommandRepository commandRepo,
        IContractPOQueryRepository releaseQueryRepo,
        IContractPOMasterQueryRepository queryRepo,
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
        _queryRepo = queryRepo;
        _mediator = mediator;
        _documentSequenceLookup = documentSequenceLookup;
        _ipAddressService = ipAddressService;
        _tz = tz;
        _outboxEventPublisher = outboxEventPublisher;
        _budgetAllocationLookup = budgetAllocationLookup;
        _misc = misc;
    }

    public async Task<ApiResponseDTO<int>> Handle(
        CreateContractPOCommand request, CancellationToken ct)
    {
        var r = request.Data;

        // Fetch contract to get VendorId, CurrencyId
        var contract = await _queryRepo.GetByIdAsync(r.ContractPOHeaderId, ct)
            ?? throw new ExceptionRules("Contract PO not found.");

        // Ensure the contract is approved before allowing release PO creation
        if (contract.StatusName != MiscEnumEntity.Approved)
            throw new ExceptionRules("This contract is not yet approved. Cannot create Release PO.");

        // Validate balance: release quantity must not exceed contract balance
        var balanceErrors = new List<string>();
        foreach (var item in r.Details)
        {
            var balance = await _releaseQueryRepo.GetContractDetailBalanceAsync(item.ContractPODetailId);
            if (item.Quantity > balance)
            {
                balanceErrors.Add(
                    $"Item {item.ItemSno}: Release quantity ({item.Quantity}) exceeds available contract balance ({balance}) for Contract Detail Id {item.ContractPODetailId}.");
            }
        }
        if (balanceErrors.Count > 0)
            throw new ExceptionRules(string.Join(" ", balanceErrors));

        // Generate PONumber from DocumentSequence
        var unitId = _ipAddressService.GetUnitId()
            ?? throw new ExceptionRules("UnitId is not available for the current user.");

        // Determine transaction type based on PO category (Emergency overrides default)
        var poCategory = await _misc.GetByIdAsync(r.POCategoryId);
        var isEmergency = string.Equals(poCategory?.Description, MiscEnumEntity.EmergencyPO, StringComparison.OrdinalIgnoreCase);
        var transactionTypeName = isEmergency
            ? MiscEnumEntity.TransactionTypeEPO
            : MiscEnumEntity.TransactionTypeCPO;

        var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
            transactionTypeName, MiscEnumEntity.ModulePurchase, unitId)
            ?? throw new ExceptionRules("No transaction type configured for Contract PO.");

        var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
        var poNumber = sequences.Count > 0
            ? sequences[^1]
            : throw new ExceptionRules("No document sequence configured for Contract PO.");

       
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
                    Message = "Cannot create Contract Release PO. Budget amount less than PO value. Budget Balance: " + currentRemaining,
                    Data = 0
                };
            }
        }

        // Timezone
        var tzId = _tz.GetSystemTimeZone();
        if (string.IsNullOrWhiteSpace(tzId) || tzId.Equals("India Standard Time", StringComparison.OrdinalIgnoreCase))
            tzId = "Asia/Kolkata";
        TimeZoneInfo tzi; try { tzi = TimeZoneInfo.FindSystemTimeZoneById(tzId); } catch { tzi = TimeZoneInfo.Local; }
        var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzi);

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
            CostCenterId = r.CostCenterId,
            BudgetGroupId = r.BudgetGroupId,
            BudgetMonthId = r.BudgetMonthId,
            BudgetRequestById = r.BudgetRequestById,
            ProjectId = r.ProjectId,
            WBSId = r.WBSId,
            FinancialYearId = r.FinancialYearId
        };

        // Build PurchaseContractHeader
        var contractHeader = new PurchaseContractHeader
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

        // Build PaymentTerms (linked to PurchaseOrderHeader via PurchaseOrderId)
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

        // Build PurchaseContractDetail + ContractPOReleaseHistory entries
        var contractDetails = new List<PurchaseContractDetail>();
        var releaseHistories = new List<ContractPOReleaseHistory>();

        foreach (var item in r.Details)
        {
            contractDetails.Add(new PurchaseContractDetail
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

            releaseHistories.Add(new ContractPOReleaseHistory
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
                    poHeader, contractHeader, contractDetails, releaseHistories, paymentTerms, ct);

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
                            Message = "Budget allocation failed. Contract Release PO creation rolled back.",
                            Data = 0
                        };
                    }
                }

                // ── Approval workflow (outbox — same transaction) ──────────────────
                var correlationId = Guid.NewGuid();

                var workFlowEntity = await _commandRepo.GetByIdContractPOWorkFlowAsync(newPOId);
                var reversePayload = new CreateContractPOReverseDto
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
                    actionCode: "CONTRACT_RELEASE_PO_CREATE",
                    actionName: poNumber,
                    details: $"Contract Release PO '{poNumber}' created against Contract '{contract.ContractPONumber}' with Id {newPOId}.",
                    module: "ContractPO"
                );
                await _mediator.Publish(ev, ct);

                return new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Message = $"Contract Release PO '{poNumber}' created successfully.",
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
