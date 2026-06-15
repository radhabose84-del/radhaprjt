#nullable disable

using AutoMapper;
using Contracts.Commands.Purchase;
using Contracts.Dtos.Common;
using Contracts.Dtos.Purchase;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.Common.Interfaces.IMRS;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.Common.Interfaces.IIssueReturn;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.GRN.StockLedger;
using PurchaseManagement.Domain.Entities.IssueReturn;
using PurchaseManagement.Domain.Entities.MRS;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using PurchaseManagement.Application.PurchaseOrder.Reports;
using Contracts.Events.Purchase;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Operations.MaintenanceManagement;

namespace PurchaseManagement.Application.Consumers
{
    public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedPurchaseCommand>
    {
        private readonly IPurchaseIndentCommand _purchaseIndentCommand;
        private readonly IMapper _imapper;        
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly ILogger<ApprovedRejectedConsumer> _logger;
        private readonly IQuotationCompareCommandRepository _quotationCompareCommand;
        private readonly IPriceMasterCommandRepository _priceMasterCommand;
        private readonly IPurchaseOrderCommandRepository _poLocalCommand;
        private readonly IPurchaseOrderQueryRepository _poLocalQuery;
        private readonly IMediator _mediator;
        private readonly IMrsEntryCommandRepository _mrsEntryCommandRepository;
        private readonly IServicePurchaseOrderCommandRepository _servicePurchaseOrderCommandRepository;
        private readonly IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
        private readonly IMaintenanceRequestConversionService _maintenanceRequestConversionService;
        private readonly IIssueReturnEntryCommandRepository _issueReturnEntryCommandRepository;
        private readonly IIssueReturnEntryQueryRepository _issueReturnEntryQueryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBudgetAllocationLookup _budgetAllocationLookup;
        private readonly IContractPOMasterCommandRepository _contractPOMasterCommandRepo;
        private readonly IBlanketMasterCommandRepository _blanketMasterCommandRepo;
        private readonly ITransactionTypeLookup _transactionTypeLookup;
        private readonly IOCREntryCommandRepository _ocrCommandRepo;
        private readonly IFreightRfqCommandRepository _freightRfqCommandRepo;

        public ApprovedRejectedConsumer(
            IPurchaseIndentCommand purchaseIndentCommand,
            IMapper mapper,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            ILogger<ApprovedRejectedConsumer> logger,
            IQuotationCompareCommandRepository quotationCompareCommand,
            IPriceMasterCommandRepository priceMasterCommand,
            IPurchaseOrderCommandRepository poLocalCommand,
            IPurchaseOrderQueryRepository poLocalQuery,
            IMediator mediator,
            IMrsEntryCommandRepository mrsEntryCommandRepository,
            IServicePurchaseOrderCommandRepository servicePurchaseOrderCommandRepository,
            IServicePurchaseOrderQueryRepository servicePurchaseOrderQueryRepository,
            IMaintenanceRequestConversionService maintenanceRequestConversionService,
            IIssueReturnEntryCommandRepository issueReturnEntryCommandRepository,
            IIssueReturnEntryQueryRepository issueReturnEntryQueryRepository,
            IHttpContextAccessor httpContextAccessor,
            IBudgetAllocationLookup budgetAllocationLookup,
            IContractPOMasterCommandRepository contractPOMasterCommandRepo,
            IBlanketMasterCommandRepository blanketMasterCommandRepo,
            ITransactionTypeLookup transactionTypeLookup,
            IOCREntryCommandRepository ocrCommandRepo,
            IFreightRfqCommandRepository freightRfqCommandRepo)
        {
            _purchaseIndentCommand = purchaseIndentCommand;
            _imapper = mapper;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _logger = logger;
            _quotationCompareCommand = quotationCompareCommand;
            _priceMasterCommand = priceMasterCommand;
            _poLocalCommand = poLocalCommand;
            _poLocalQuery = poLocalQuery;
            _mediator = mediator;
            _mrsEntryCommandRepository = mrsEntryCommandRepository;
            _servicePurchaseOrderCommandRepository = servicePurchaseOrderCommandRepository;
            _servicePurchaseOrderQueryRepository = servicePurchaseOrderQueryRepository;
            _maintenanceRequestConversionService = maintenanceRequestConversionService;
            _issueReturnEntryCommandRepository = issueReturnEntryCommandRepository;
            _issueReturnEntryQueryRepository = issueReturnEntryQueryRepository;
            _httpContextAccessor = httpContextAccessor;
            _budgetAllocationLookup = budgetAllocationLookup;
            _contractPOMasterCommandRepo = contractPOMasterCommandRepo;
            _blanketMasterCommandRepo = blanketMasterCommandRepo;
            _transactionTypeLookup = transactionTypeLookup;
            _ocrCommandRepo = ocrCommandRepo;
            _freightRfqCommandRepo = freightRfqCommandRepo;
        }

        public async Task Consume(ConsumeContext<UpdateApprovedRejectedPurchaseCommand> context)
        {
            var msg = context.Message;

            // ✅ Null-safe defaults
            var lineStatus = msg.LineStatus ?? new List<UpdateLineStatusDto>();
            var partyContacts = msg.PartyContacts ?? new List<PartyRefDto>();
            var dynamicFields = msg.DynamicFields ?? new List<JsonElement>();

            try
            {
                _logger.LogInformation("Purchase Consumer Approval Status Update: {@Message}", msg);

                // Helper: publish completion for saga
                async Task PublishCompletedAsync()
                {
                    await context.Publish(new ApprovedRejectedPurchaseCompletedEvent
                    {
                        CorrelationId = msg.CorrelationId,
                        ModuleTransactionId = msg.ModuleTransactionId
                      
                    });
                }

                // -----------------------------
                // PURCHASE INDENT
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.PurchaseIndent)
                {
                    var status = msg.Status;

                    var approved = await _miscMasterQueryRepository
                        .GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Approved);
                    var rejected = await _miscMasterQueryRepository
                        .GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Rejected);
                    var pending = await _miscMasterQueryRepository
                        .GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Pending);

                    // Header comes from ModuleTransactionId
                    var indentHeader = new IndentHeader
                    {
                        Id = msg.ModuleTransactionId
                    };

                    // Set header status
                    if (status == MiscEnumEntity.Pending)
                        indentHeader.StatusId = pending.Id;
                    else if (status == MiscEnumEntity.Approved)
                        indentHeader.StatusId = approved.Id;
                    else if (status == MiscEnumEntity.Rejected)
                        indentHeader.StatusId = rejected.Id;

                    var lineStatusList = msg.LineStatus ?? new List<UpdateLineStatusDto>();
                    var detailUpdates = new List<IndentDetail>();

                    // Build detail updates ONLY if we have valid lineIds
                    foreach (var ls in lineStatusList)
                    {
                        // In many cases ModuleLineId is actually the header id (1078),
                        // so ignore those – they are not real detail ids.
                        if (ls.ModuleLineId <= 0 || ls.ModuleLineId == msg.ModuleTransactionId)
                            continue;

                        var detailStatusId =
                            ls.Status == MiscEnumEntity.Approved ? approved.Id :
                            ls.Status == MiscEnumEntity.Rejected ? rejected.Id :
                            pending.Id;

                        detailUpdates.Add(new IndentDetail
                        {
                            Id = ls.ModuleLineId,     // must match Purchase.IndentDetail.Id
                            StatusId = detailStatusId
                        });
                    }

                    // If we have valid detail rows, send them; otherwise let repo update ALL details to header status
                    indentHeader.IndentDetails = detailUpdates;

                    await _purchaseIndentCommand.FinalizeStatus(indentHeader);
                    await PublishCompletedAsync();
                    return;
                }


                // -----------------------------
                // QUOTATION COMPARISON
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.QuotationComparison)
                {
                    var status = msg.Status;
                    var rfqId = msg.ModuleTransactionId;

                    if (status == MiscEnumEntity.Approved || status == MiscEnumEntity.Rejected)
                    {
                        var approved = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
                        var rejected = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

                        var finalStatusId = status == MiscEnumEntity.Approved ? approved.Id : rejected.Id;
                        await _quotationCompareCommand.UpdateQuoteApproveAsync(rfqId, finalStatusId);
                    }

                    await PublishCompletedAsync();
                    return;
                }

                // -----------------------------
                // PRICE MASTER
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.PriceMaster)
                {
                    var status = msg.Status;
                    var priceMasterId = msg.ModuleTransactionId;

                    if (status == MiscEnumEntity.Approved || status == MiscEnumEntity.Rejected)
                    {
                        var approved = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
                        var rejected = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

                        var finalStatusId = status == MiscEnumEntity.Approved ? approved.Id : rejected.Id;
                        await _priceMasterCommand.UpdatePriceMasterApproveAsync(priceMasterId, finalStatusId);
                    }

                    await PublishCompletedAsync();
                    return;
                }

                // -----------------------------
                // PURCHASE ORDER (all types: LPO, CPO, IPO, EPO, BPO)
                // PO handlers publish with ModuleTypeName = the PO sub-type's TransactionType name.
                // TransactionTypeId differentiates the PO sub-type.
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.TransactionTypeLPO
                    || msg.ModuleTypeName == MiscEnumEntity.TransactionTypeCPO
                    || msg.ModuleTypeName == MiscEnumEntity.TransactionTypeIPO
                    || msg.ModuleTypeName == MiscEnumEntity.TransactionTypeEPO
                    || msg.ModuleTypeName == MiscEnumEntity.TransactionTypeBPO)
                {
                    var status = msg.Status;
                    var poId = msg.ModuleTransactionId;

                    // Resolve TransactionTypeId → TypeName to branch per PO sub-type
                    string transactionTypeName = null;
                    if (msg.TransactionTypeId.HasValue)
                    {
                        var txTypes = await _transactionTypeLookup.GetByIdsAsync(new[] { msg.TransactionTypeId.Value });
                        transactionTypeName = txTypes.FirstOrDefault()?.TypeName;
                    }

                    _logger.LogInformation(
                        "PO approval: PoId={PoId}, Status={Status}, TransactionTypeId={TxTypeId}, TransactionType={TxType}",
                        poId, status, msg.TransactionTypeId, transactionTypeName ?? "Unknown");

                    if (status == MiscEnumEntity.Approved || status == MiscEnumEntity.Rejected)
                    {
                        var approved = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
                        var rejected = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);
                        var finalStatusId = status == MiscEnumEntity.Approved ? approved.Id : rejected.Id;

                        // Status update — common for ALL PO types (LPO, IPO, CPO, EPO)
                        var updated = await _poLocalCommand.UpdatePOApproveAsync(poId, finalStatusId, context.CancellationToken);
                        if (!updated)
                            throw new Exception($"PO approval update failed for PoId={poId}, TransactionTypeId={msg.TransactionTypeId}");

                /*         // ── LPO-SPECIFIC: PDF email on approval ──
                        if (status == MiscEnumEntity.Approved
                            && string.Equals(transactionTypeName, MiscEnumEntity.TransactionTypeLPO, StringComparison.OrdinalIgnoreCase))
                        {
                            var unitId = partyContacts.FirstOrDefault()?.UnitId ?? 0;

                            await _mediator.Send(new QueuePoPdfEmailCommand(
                                UnitId: unitId,
                                PoId: poId,
                                PartyContacts: partyContacts,
                                RowsJson: null
                            ), context.CancellationToken);
                        } */

                        // ── LPO-SPECIFIC: BUDGET REVERSAL ON REJECTION ──
                        // When LPO is rejected, add back the PurchaseValue to the
                        // BudgetAllocation remaining balance (reverse the deduction
                        // that was made atomically during PO creation).
                        if (status == MiscEnumEntity.Rejected
                            && string.Equals(transactionTypeName, MiscEnumEntity.TransactionTypeLPO, StringComparison.OrdinalIgnoreCase))
                        {
                            var poDetail = await _poLocalQuery.GetByIdAsync(poId, context.CancellationToken);

                            if (poDetail != null
                                && poDetail.BudgetGroupId.HasValue
                                && poDetail.BudgetGroupId.Value > 0
                                && poDetail.PurchaseValue > 0)
                            {
                                var budgetMonthDate = new DateOnly(
                                    poDetail.PODate.Year, poDetail.PODate.Month, 1);

                                var reversed = await _budgetAllocationLookup
                                    .ApplyRemainingBalanceDeltaAsync(
                                        budgetGroupId: poDetail.BudgetGroupId.Value,
                                        budgetDate: budgetMonthDate,
                                        monthId: poDetail.BudgetMonthId ?? 0,
                                        requestById: poDetail.BudgetRequestById ?? 0,
                                        deltaAmount: poDetail.PurchaseValue, // positive = adds back
                                        projectId: poDetail.ProjectId,
                                        wbsId: poDetail.WBSId,
                                        financialYearId: poDetail.FinancialYearId,
                                        ct: context.CancellationToken);

                                if (reversed)
                                {
                                    _logger.LogInformation(
                                        "Budget reversed for rejected PO {PoId}. Amount {Amount} added back to BudgetGroup {BgId}.",
                                        poId, poDetail.PurchaseValue, poDetail.BudgetGroupId.Value);
                                }
                                else
                                {
                                    _logger.LogWarning(
                                        "Budget reversal failed for rejected PO {PoId}. BudgetGroup {BgId}, Amount {Amount}.",
                                        poId, poDetail.BudgetGroupId.Value, poDetail.PurchaseValue);
                                }
                            }
                        }
                    }

                    await PublishCompletedAsync();
                    return;
                }

                // -----------------------------
                // MATERIAL REQUEST (MRS)
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.MaterialRequest)
                {
                    var status = msg.Status;
                    var rfqId = msg.ModuleTransactionId;

                    if (status == MiscEnumEntity.Approved || status == MiscEnumEntity.Rejected)
                    {
                        var approved = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
                        var rejected = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

                        var finalStatusId = status == MiscEnumEntity.Approved ? approved.Id : rejected.Id;
                        await _mrsEntryCommandRepository.UpdateMrsApproveAsync(rfqId, finalStatusId);
                    }

                    await PublishCompletedAsync();
                    return;
                }

                // -----------------------------
                // SERVICE PO
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.ServicePO)
                {
                    var status = msg.Status;
                    var poId = msg.ModuleTransactionId;

                    if (status == MiscEnumEntity.Approved || status == MiscEnumEntity.Rejected)
                    {
                        var approved = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
                        var rejected = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);
                        var finalStatusId = status == MiscEnumEntity.Approved ? approved.Id : rejected.Id;

                        var updated = await _servicePurchaseOrderCommandRepository.UpdateServicePOApproveAsync(poId, finalStatusId, context.CancellationToken);
                        if (!updated)
                            throw new Exception($"Service PO approval update failed for PoId={poId}");

                        // ── EXTERNAL MAINTENANCE REQUEST CONVERSION ──
                        // On Approved only: for each linked MaintenanceRequest in this PO's lines,
                        // bump its ConvertedToPoAmount and flip status (Open → PartiallyConverted → FullyConverted).
                        // Rejected POs leave the linked request untouched.
                        if (status == MiscEnumEntity.Approved)
                        {
                            var linked = await _servicePurchaseOrderQueryRepository
                                .GetLinkedMaintenanceRequestsAsync(poId, context.CancellationToken);

                            foreach (var link in linked)
                            {
                                var applied = await _maintenanceRequestConversionService
                                    .ApplyServicePoConversionAsync(
                                        link.RequestId,
                                        link.TotalPlannedValue,
                                        context.CancellationToken);

                                if (!applied)
                                {
                                    _logger.LogWarning(
                                        "Service PO {PoId} approved but conversion update did not affect MaintenanceRequest {RequestId} (Δ={Delta}). " +
                                        "Request may be missing or status MiscMaster code unresolved.",
                                        poId, link.RequestId, link.TotalPlannedValue);
                                }
                                else
                                {
                                    _logger.LogInformation(
                                        "MaintenanceRequest {RequestId} converted by {Delta} via Service PO {PoId}.",
                                        link.RequestId, link.TotalPlannedValue, poId);
                                }
                            }
                        }
                    }

                    await PublishCompletedAsync();
                    return;
                }

                // -----------------------------
                // SERVICE ENTRY SHEET
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.ServiceEntrySheet)
                {
                    var status = msg.Status;
                    var sesId = msg.ModuleTransactionId;

                    if (status == MiscEnumEntity.Approved || status == MiscEnumEntity.Rejected)
                    {
                        var approved = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
                        var rejected = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

                        var finalStatusId = status == MiscEnumEntity.Approved ? approved.Id : rejected.Id;

                        var updated = await _servicePurchaseOrderCommandRepository
                            .UpdateServiceEntrySheetApproveAsync(sesId, finalStatusId, context.CancellationToken);

                        if (!updated)
                            throw new Exception($"Service Entry Sheet approval update failed for SES={sesId}");
                    }

                    await PublishCompletedAsync();
                    return;
                }

                // -----------------------------
                // ISSUE RETURN
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.IssueReturn)
                {
                    var status = msg.Status;

                    // Only care about Approved / Rejected
                    if (status != MiscEnumEntity.Approved && status != MiscEnumEntity.Rejected)
                    {
                        await PublishCompletedAsync();
                        return;
                    }

                    var approved = await _miscMasterQueryRepository
                        .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
                    var rejected = await _miscMasterQueryRepository
                        .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

                    var finalStatusId = status == MiscEnumEntity.Approved ? approved.Id : rejected.Id;

                    // 🔹 Header comes from ModuleTransactionId
                    var issueReturnHeader = new IssueReturnHeader
                    {
                        Id = msg.ModuleTransactionId,
                        StatusId = finalStatusId
                    };

                    // 🔹 Build detail updates from LineStatus (if any)
                    var detailUpdates = new List<IssueReturnDetail>();   // use your actual detail entity type

                    var lineStatusList = msg.LineStatus ?? new List<UpdateLineStatusDto>();

                    foreach (var ls in lineStatusList)
                    {
                        // ModuleLineId must be the IssueReturnDetail.Id
                        if (ls.ModuleLineId <= 0)
                            continue;

                        var detailStatusId =
                            ls.Status == MiscEnumEntity.Approved ? approved.Id :
                            ls.Status == MiscEnumEntity.Rejected ? rejected.Id :
                            finalStatusId; // fallback

                        detailUpdates.Add(new IssueReturnDetail
                        {
                            Id = ls.ModuleLineId,
                            StatusId = detailStatusId
                        });
                    }

                    // If empty -> repo will treat as "update ALL details to header.StatusId"
                    issueReturnHeader.IssueReturnDetailsHeaderName = detailUpdates;

                    var updated = await _issueReturnEntryCommandRepository.FinalizeStatus(issueReturnHeader);
                    if (!updated)
                        throw new Exception($"IssueReturn FinalizeStatus failed for Id={issueReturnHeader.Id}");

                    // 🔻 BELOW: keep your existing stock-ledger logic as-is
                    var getIssueReturnDetails =
                        await _issueReturnEntryQueryRepository.GetByIdWithDetails(issueReturnHeader.Id);

                    static int ReadInt(JsonElement e, string prop)
                        => e.ValueKind == JsonValueKind.Object &&
                        e.TryGetProperty(prop, out var p) &&
                        p.ValueKind == JsonValueKind.Number
                                ? p.GetInt32()
                                : 0;

                    if (getIssueReturnDetails.RequestCategoryName.Equals(MiscEnumEntity.Consumption, StringComparison.OrdinalIgnoreCase) &&
                        getIssueReturnDetails.StatusName.Equals(MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase))
                    {
                        var approvedLines = getIssueReturnDetails.getIssueReturnDetails
                            .Where(x => x.StatusName == MiscEnumEntity.Approved)
                            .ToList();

                        foreach (var line in approvedLines)
                        {
                            JsonElement? match = dynamicFields.FirstOrDefault(df =>
                                df.ValueKind == JsonValueKind.Object &&
                                df.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number && idProp.GetInt32() == line.Id &&
                                df.TryGetProperty("issueReturnHeaderId", out var hdrProp) && hdrProp.ValueKind == JsonValueKind.Number && hdrProp.GetInt32() == line.IssueReturnHeaderId &&
                                df.TryGetProperty("itemId", out var itemProp) && itemProp.ValueKind == JsonValueKind.Number && itemProp.GetInt32() == line.ItemId
                            );

                            int warehouseId = 0, storageTypeId = 0, targetId = 0;

                            if (match.HasValue)
                            {
                                warehouseId = ReadInt(match.Value, "warehouseId");
                                storageTypeId = ReadInt(match.Value, "storageTypeId");
                                targetId = ReadInt(match.Value, "targetId");
                            }

                            var ledger = new StockLedger
                            {
                                UnitId = getIssueReturnDetails.UnitId,
                                DocType = "RET",
                                DocNo = getIssueReturnDetails.Id,
                                DocSlNo = line.Id,
                                DocDate = DateTime.Today,
                                ItemId = line.ItemId,
                                UomId = line.UomId,
                                WarehouseId = warehouseId,
                                StorageTypeId = storageTypeId,
                                TargetId = targetId,
                                ReceivedQty = line.ReturnQuantity,
                                ReceivedValue = line.ReturnValue,
                                IssueQty = 0,
                                IssueValue = 0
                            };

                            await _issueReturnEntryCommandRepository.InsertAsync(ledger);
                        }

                        await PublishCompletedAsync();
                        return;
                    }

                    if (getIssueReturnDetails.RequestCategoryName.Equals(MiscEnumEntity.SubStores, StringComparison.OrdinalIgnoreCase) &&
                        getIssueReturnDetails.StatusName.Equals(MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase))
                    {
                        var approvedLines = getIssueReturnDetails.getIssueReturnDetails
                            .Where(x => x.StatusName == MiscEnumEntity.Approved)
                            .ToList();

                        foreach (var line in approvedLines)
                        {
                            JsonElement? matchedDynamic = dynamicFields.FirstOrDefault(df =>
                                df.ValueKind == JsonValueKind.Object &&
                                df.TryGetProperty("itemId", out var it) && it.ValueKind == JsonValueKind.Number && it.GetInt32() == line.ItemId &&
                                df.TryGetProperty("issueReturnHeaderId", out var ih) && ih.ValueKind == JsonValueKind.Number && ih.GetInt32() == line.IssueReturnHeaderId &&
                                df.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.Number && id.GetInt32() == line.Id
                            );

                            int dynamicWarehouseId = matchedDynamic.HasValue ? ReadInt(matchedDynamic.Value, "warehouseId") : 0;
                            int dynamicStorageTypeId = matchedDynamic.HasValue ? ReadInt(matchedDynamic.Value, "storageTypeId") : 0;
                            int dynamicTargetId = matchedDynamic.HasValue ? ReadInt(matchedDynamic.Value, "targetId") : 0;

                            var ledger = new StockLedger
                            {
                                UnitId = getIssueReturnDetails.UnitId,
                                DocType = "RET",
                                DocNo = getIssueReturnDetails.Id,
                                DocSlNo = line.Id,
                                DocDate = DateTime.Today,
                                ItemId = line.ItemId,
                                UomId = line.UomId,
                                WarehouseId = dynamicWarehouseId,
                                StorageTypeId = dynamicStorageTypeId,
                                TargetId = dynamicTargetId,
                                ReceivedQty = line.ReturnQuantity,
                                ReceivedValue = line.ReturnValue,
                                IssueQty = 0,
                                IssueValue = 0
                            };

                            var subStoreLedger = new SubStoreStockLedger
                            {
                                UnitId = getIssueReturnDetails.UnitId,
                                DocType = "RTM",
                                DocNo = getIssueReturnDetails.Id,
                                DocSlNo = line.Id,
                                DocDate = DateTime.Today,
                                ItemId = line.ItemId,
                                UomId = line.UomId,
                                DepartmentId = line.SubStoresDepartmentId ?? 0,
                                WarehouseId = line.WarehouseStockId,
                                StorageTypeId = line.StorageTypeId,
                                TargetId = line.TargetId,
                                IssueQty = line.ReturnQuantity,
                                IssueValue = line.ReturnValue
                            };

                            await _issueReturnEntryCommandRepository.InsertStockAsync(ledger, subStoreLedger);
                        }

                        await PublishCompletedAsync();
                        return;
                    }

                    await PublishCompletedAsync();
                    return;
                }


                // -----------------------------
                // CONTRACT PO MASTER
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.TransactionTypeContract)
                {
                    var status = msg.Status;
                    var contractPOId = msg.ModuleTransactionId;

                    if (status == MiscEnumEntity.Approved || status == MiscEnumEntity.Rejected)
                    {
                        var approved = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
                        var rejected = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

                        var finalStatusId = status == MiscEnumEntity.Approved ? approved.Id : rejected.Id;

                        var updated = await _contractPOMasterCommandRepo.UpdateContractPOApproveAsync(
                            contractPOId, finalStatusId, context.CancellationToken);

                        if (!updated)
                            throw new Exception($"Contract PO Master approval update failed for Id={contractPOId}");
                    }

                    await PublishCompletedAsync();
                    return;
                }

                // -----------------------------
                // BLANKET MASTER
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.TransactionTypeBlanket)
                {
                    var status = msg.Status;
                    var blanketId = msg.ModuleTransactionId;

                    if (status == MiscEnumEntity.Approved || status == MiscEnumEntity.Rejected)
                    {
                        var approved = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
                        var rejected = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

                        var finalStatusId = status == MiscEnumEntity.Approved ? approved.Id : rejected.Id;

                        var updated = await _blanketMasterCommandRepo.UpdateBlanketApproveAsync(
                            blanketId, finalStatusId, context.CancellationToken);

                        if (!updated)
                            throw new Exception($"Blanket Master approval update failed for Id={blanketId}");
                    }

                    await PublishCompletedAsync();
                    return;
                }

                // -----------------------------
                // PURCHASE RETURN (RTV)
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.RtvModuleTypeName)
                {
                    var status = msg.Status;
                    var rtvId = msg.ModuleTransactionId;

                    if (status == MiscEnumEntity.Approved || status == MiscEnumEntity.Rejected)
                    {
                        await _mediator.Send(new PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.ProcessApprovalDecision.ProcessPurchaseReturnApprovalDecisionCommand(
                            PurchaseReturnHeaderId: rtvId,
                            IsApproved: status == MiscEnumEntity.Approved,
                            ApprovalRequestId: null,
                            ApproverRemarks: null), context.CancellationToken);
                    }

                    await PublishCompletedAsync();
                    return;
                }

                // -----------------------------
                // OCR ENTRY
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.TransactionTypeOCR)
                {
                    var status = msg.Status;
                    var ocrId = msg.ModuleTransactionId;

                    if (status == MiscEnumEntity.Approved || status == MiscEnumEntity.Rejected)
                    {
                        var approved = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
                        var rejected = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

                        var finalStatusId = status == MiscEnumEntity.Approved ? approved.Id : rejected.Id;

                        var updated = await _ocrCommandRepo.UpdateOcrApproveAsync(
                            ocrId, finalStatusId, context.CancellationToken);

                        if (!updated)
                            throw new Exception($"OCR approval update failed for Id={ocrId}");
                    }

                    await PublishCompletedAsync();
                    return;
                }

                // -----------------------------
                // FREIGHT RFQ
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.TransactionTypeFreightRfq)
                {
                    var status = msg.Status;
                    var rfqId = msg.ModuleTransactionId;

                    if (status == MiscEnumEntity.Approved)
                    {
                        // Sets FreightRfqStatus = Approved and snapshots the selected transporter/rate/freight.
                        var affected = await _freightRfqCommandRepo.ApproveAsync(rfqId, null);
                        if (affected <= 0)
                            throw new InvalidOperationException($"Freight RFQ approval update failed for Id={rfqId}");
                    }
                    else if (status == MiscEnumEntity.Rejected)
                    {
                        var affected = await _freightRfqCommandRepo.RejectAsync(rfqId, null);
                        if (affected <= 0)
                            throw new InvalidOperationException($"Freight RFQ rejection update failed for Id={rfqId}");
                    }

                    await PublishCompletedAsync();
                    return;
                }

                // ✅ If module not handled, still complete so saga doesn't hang
                await PublishCompletedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Purchase Consumer Approval Status failed. {@Message}", msg);

                // ✅ Inform saga
                await context.Publish(new ApprovedRejectedFailedEvent
                {
                    CorrelationId = msg.CorrelationId,
                    IndentId = msg.ModuleTransactionId,                    
                    Reason = $"Unhandled ModuleTypeName={msg.ModuleTypeName} and status={msg.Status} and Message={ex.Message}",
                    LineStatus = msg.LineStatus
                });
                throw;
            }
        }
    }
}
