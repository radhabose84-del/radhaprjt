using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Update;

public sealed class UpdateContractPOCommandHandler
    : IRequestHandler<UpdateContractPOCommand, bool>
{
    private readonly IContractPOCommandRepository _commandRepo;
    private readonly IContractPOQueryRepository _releaseQueryRepo;
    private readonly IContractPOMasterQueryRepository _queryRepo;
    private readonly IMediator _mediator;
    private readonly IIPAddressService _ipAddressService;
    private readonly ITimeZoneService _tz;
    private readonly IBudgetAllocationLookup _budgetAllocationLookup;

    public UpdateContractPOCommandHandler(
        IContractPOCommandRepository commandRepo,
        IContractPOQueryRepository releaseQueryRepo,
        IContractPOMasterQueryRepository queryRepo,
        IMediator mediator,
        IIPAddressService ipAddressService,
        ITimeZoneService tz,
        IBudgetAllocationLookup budgetAllocationLookup)
    {
        _commandRepo = commandRepo;
        _releaseQueryRepo = releaseQueryRepo;
        _queryRepo = queryRepo;
        _mediator = mediator;
        _ipAddressService = ipAddressService;
        _tz = tz;
        _budgetAllocationLookup = budgetAllocationLookup;
    }

    public async Task<bool> Handle(UpdateContractPOCommand request, CancellationToken ct)
    {
        var r = request.Data;

        // Fetch existing PO to get PONumber for document renaming
        var existing = await _releaseQueryRepo.GetContractPOByIdAsync(r.Id, ct)
            ?? throw new ExceptionRules("Contract Release PO not found.");

        // Fetch contract to get VendorId, CurrencyId
        var contract = await _queryRepo.GetByIdAsync(r.ContractPOHeaderId, ct)
            ?? throw new ExceptionRules("Contract PO not found.");

        // Ensure the contract is approved before allowing release PO update
        if (contract.StatusName != MiscEnumEntity.Approved)
            throw new ExceptionRules("This contract is not yet approved. Cannot update Release PO.");

        // Validate balance: release quantity must not exceed contract balance
        // For update, add back old PO's quantities since they will be reversed
        var balanceErrors = new List<string>();
        foreach (var item in r.Details)
        {
            var currentBalance = await _releaseQueryRepo.GetContractDetailBalanceAsync(item.ContractPODetailId);
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

        // -------------------------------------------------
        // BUDGET VALIDATION (BudgetGroup vs PurchaseValue)
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
                throw new ExceptionRules("Cannot update Contract Release PO. Budget amount less than PO value. Budget Balance: " + currentRemaining);
        }

        // Resolve UnitId from logged-in user
        var unitId = _ipAddressService.GetUnitId()
            ?? throw new ExceptionRules("UnitId is not available for the current user.");

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
                    var finalName = $"{existing.PONumber}_{doc.DocumentId}{Path.GetExtension(oldPath)}";
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

        // Build PurchaseOrderHeader
        var poHeader = new PurchaseOrderHeader
        {
            Id = r.Id,
            UnitId = unitId,
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
            StatusId = r.StatusId,
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

        // Build detail + release history entries
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

        var updatedId = await _commandRepo.UpdateContractPOAsync(
            poHeader, contractHeader, contractDetails, releaseHistories, paymentTerms, ct);

        // Audit
        var ev = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: "CONTRACT_RELEASE_PO_UPDATE",
            actionName: r.Id.ToString(),
            details: $"Contract Release PO with Id {r.Id} updated successfully.",
            module: "ContractPO"
        );
        await _mediator.Publish(ev, ct);

        return updatedId > 0;
    }
}
