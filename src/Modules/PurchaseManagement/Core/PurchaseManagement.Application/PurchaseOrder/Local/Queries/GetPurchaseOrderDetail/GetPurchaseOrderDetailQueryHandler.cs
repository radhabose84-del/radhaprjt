using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderDetail;

public class GetPurchaseOrderDetailQueryHandler
    : IRequestHandler<GetPurchaseOrderDetailQuery, PurchaseOrderDetailWithSummaryDto?>
{
    private const string StatusCompleted = "Completed";
    private const string StatusPending = "Pending";

    private readonly IPurchaseOrderQueryRepository _repo;
    private readonly IIPAddressService _ip;
    private readonly IPartyLookup _partyLookup;
    private readonly IPartyDetailLookup _partyDetailLookup;
    private readonly ICurrencyLookup _currencyLookup;
    private readonly IUOMLookup _uomLookup;
    private readonly IItemLookup _itemLookup;
    private readonly IDepartmentLookup _departmentLookup;
    private readonly ICompanyLookup _companyLookup;
    private readonly IUnitLookup _unitLookup;
    private readonly IBudgetAllocationLookup _budgetAllocationLookup;
    private readonly IWorkflowLookup _workflowLookup;
    private readonly IUserLookup _userLookup;

    public GetPurchaseOrderDetailQueryHandler(
        IPurchaseOrderQueryRepository repo,
        IIPAddressService ip,
        IPartyLookup partyLookup,
        IPartyDetailLookup partyDetailLookup,
        ICurrencyLookup currencyLookup,
        IUOMLookup uomLookup,
        IItemLookup itemLookup,
        IDepartmentLookup departmentLookup,
        ICompanyLookup companyLookup,
        IUnitLookup unitLookup,
        IBudgetAllocationLookup budgetAllocationLookup,
        IWorkflowLookup workflowLookup,
        IUserLookup userLookup)
    {
        _repo = repo;
        _ip = ip;
        _partyLookup = partyLookup;
        _partyDetailLookup = partyDetailLookup;
        _currencyLookup = currencyLookup;
        _uomLookup = uomLookup;
        _itemLookup = itemLookup;
        _departmentLookup = departmentLookup;
        _companyLookup = companyLookup;
        _unitLookup = unitLookup;
        _budgetAllocationLookup = budgetAllocationLookup;
        _workflowLookup = workflowLookup;
        _userLookup = userLookup;
    }

    public async Task<PurchaseOrderDetailWithSummaryDto?> Handle(GetPurchaseOrderDetailQuery r, CancellationToken ct)
    {
        var header = await _repo.GetDetailByIdAsync(r.Id, ct);
        if (header is null) return null;

        await EnrichDetailAsync(header, ct);

        var summary = new PoSummaryDto();
        var approverName = await BuildApprovalAndProgressAsync(header, r.Id, summary, ct);
        await BuildBudgetAsync(header, summary, ct);
        await BuildOutstandingAsync(r.Id, summary, ct);

        // Approver name is shared between the Approval panel and the Approval progress step.
        summary.Approval.ApproverName = approverName;

        return new PurchaseOrderDetailWithSummaryDto { Detail = header, Summary = summary };
    }

    // ---------------- Detail enrichment (vendor / currency / documents / line names + HSN) ----------------
    private async Task EnrichDetailAsync(PurchaseOrderDetailDto header, CancellationToken ct)
    {
        if (header.VendorId > 0)
        {
            try
            {
                var party = await _partyLookup.GetByIdAsync(header.VendorId, ct);
                if (party != null) header.VendorName = party.PartyName;
            }
            catch { /* swallow */ }

            try
            {
                var partyDetail = await _partyDetailLookup.GetByIdAsync(header.VendorId, ct);
                if (partyDetail != null)
                {
                    header.VendorCode = partyDetail.PartyCode;
                    header.VendorGSTIN = partyDetail.GSTNumber;
                    header.VendorPhone = partyDetail.Phone;
                    header.VendorMobile = partyDetail.MobileNo;
                    header.VendorCreditDays = partyDetail.CreditDays;
                }
            }
            catch { /* swallow */ }
        }

        if (header.CurrencyId > 0)
        {
            try
            {
                var currencies = await _currencyLookup.GetByIdsAsync(new[] { header.CurrencyId }, ct);
                var currency = currencies.FirstOrDefault();
                if (currency != null)
                    header.CurrencyName = !string.IsNullOrWhiteSpace(currency.Code) ? currency.Code : currency.Name;
            }
            catch { /* swallow */ }
        }

        if (header.DocumentsList?.Count > 0)
        {
            try
            {
                var companies = await _companyLookup.GetAllCompanyAsync();
                var units = await _unitLookup.GetAllUnitAsync();
                var companyLookupDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
                var unitLookupDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                companyLookupDict.TryGetValue(_ip.GetCompanyId() ?? 0, out var companyName);
                unitLookupDict.TryGetValue(_ip.GetUnitId() ?? 0, out var unitName);
                companyName ??= string.Empty;
                unitName ??= string.Empty;

                foreach (var doc in header.DocumentsList)
                    if (!string.IsNullOrWhiteSpace(doc.FileName))
                        doc.UploadedPath = $"PoDocument/{companyName}/{unitName}/{doc.FileName}";
            }
            catch { /* swallow */ }
        }

        var allDetails = header.Headers?.SelectMany(h => h.Details ?? new List<PurchaseLocalDetailDto>()).ToList()
                         ?? new List<PurchaseLocalDetailDto>();

        if (allDetails.Count == 0) return;

        var itemIds = allDetails.Select(x => x.ItemId).Where(x => x > 0).Distinct().ToList();
        var uomIds = allDetails.Select(x => x.UOMId).Where(x => x > 0).Distinct().ToList();
        var deptIds = allDetails.Select(x => x.DepartmentId ?? 0).Where(x => x > 0).Distinct().ToList();

        var items = itemIds.Count > 0
            ? await _itemLookup.GetByIdsAsync(itemIds, ct)
            : new List<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>();
        var uoms = uomIds.Count > 0
            ? await _uomLookup.GetByIdsAsync(uomIds, ct)
            : new List<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>();
        var depts = deptIds.Count > 0
            ? await _departmentLookup.GetByIdsAsync(deptIds, ct)
            : new List<Contracts.Dtos.Lookups.Users.DepartmentLookupDto>();

        var itemMap = items.ToDictionary(i => i.Id, i => !string.IsNullOrWhiteSpace(i.ItemName) ? i.ItemName : i.ItemCode);
        var hsnMap = items.ToDictionary(i => i.Id, i => i.HSNCode);
        var uomMap = uoms.ToDictionary(u => u.Id, u => !string.IsNullOrWhiteSpace(u.UOMName) ? u.UOMName : u.Code);
        var deptMap = depts.ToDictionary(d => d.DepartmentId, d => d.DepartmentName ?? string.Empty);

        foreach (var d in allDetails)
        {
            if (d.ItemId > 0 && itemMap.TryGetValue(d.ItemId, out var itemName))
                d.ItemName = itemName;
            if (d.ItemId > 0 && hsnMap.TryGetValue(d.ItemId, out var hsnCode))
                d.HSNCode = hsnCode;
            if (d.UOMId > 0 && uomMap.TryGetValue(d.UOMId, out var uomName))
                d.UOMName = uomName;
            if (d.DepartmentId is > 0 && deptMap.TryGetValue(d.DepartmentId.Value, out var deptName))
                d.DepartmentName = deptName;
        }
    }

    // ---------------- Approval panel + PO Progress timeline ----------------
    private async Task<string?> BuildApprovalAndProgressAsync(
        PurchaseOrderDetailDto header, int poId, PoSummaryDto summary, CancellationToken ct)
    {
        string? approverName = null;
        try
        {
            var moduleType = MapMethodToTransactionType(header.POMethodCode);
            var approvers = await _workflowLookup.GetApproverListAsync(moduleType, new[] { poId });
            var first = approvers.FirstOrDefault(a => int.TryParse(a.ApproverValue, out _));
            if (first != null && int.TryParse(first.ApproverValue, out var userId))
            {
                var user = await _userLookup.GetByIdAsync(userId, ct);
                approverName = user?.UserName;
            }
        }
        catch { /* swallow workflow failures */ }

        var statusCode = header.StatusCode ?? string.Empty;
        summary.Approval.Status = header.StatusCode;

        bool hasGrn = false, hasInvoice = false;
        try { hasGrn = await _repo.HasAnyGrnAsync(poId, ct); } catch { /* swallow */ }
        try { hasInvoice = await _repo.HasAnyBillEntryAsync(poId, ct); } catch { /* swallow */ }

        var approvalStepStatus =
            statusCode.Equals(MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase) ? StatusCompleted :
            statusCode.Equals(MiscEnumEntity.Rejected, StringComparison.OrdinalIgnoreCase) ? "Rejected" :
            "Awaiting";

        var steps = new List<PoProgressStepDto>
        {
            new() { Step = "PO Created",     Status = StatusCompleted, Date = header.PODate },
            new() { Step = "Approval",       Status = approvalStepStatus, Detail = approverName },
            new() { Step = "GRN Received",   Status = hasGrn ? StatusCompleted : StatusPending },
            new() { Step = "Invoice Matched", Status = hasInvoice ? StatusCompleted : StatusPending },
            // No exposed vendor-payment source — Payment Done stays Pending.
            new() { Step = "Payment Done",   Status = StatusPending },
        };

        var completed = steps.Count(s => s.Status == StatusCompleted);
        summary.Progress.Steps = steps;
        summary.Progress.PercentComplete = (int)Math.Round(completed * 100.0 / steps.Count);

        return approverName;
    }

    // ---------------- Budget panel ----------------
    private async Task BuildBudgetAsync(PurchaseOrderDetailDto header, PoSummaryDto summary, CancellationToken ct)
    {
        if (header.BudgetGroupId is not > 0) return;

        try
        {
            var budgetDate = DateOnly.FromDateTime(header.PODate.Date);
            var alloc = await _budgetAllocationLookup.GetAllocationSummaryAsync(
                header.BudgetGroupId.Value,
                budgetDate,
                header.BudgetMonthId ?? 0,
                header.BudgetRequestById ?? 0,
                header.ProjectId,
                header.WBSId,
                header.FinancialYearId,
                ct);

            if (alloc != null)
            {
                summary.Budget.HasAllocation = true;
                summary.Budget.ApprovedAmount = alloc.ApprovedAmount;
                summary.Budget.RemainingBalance = alloc.RemainingBalance;
                summary.Budget.IsPositive = alloc.RemainingBalance >= 0;
                summary.Budget.UtilisedPercent = alloc.ApprovedAmount > 0
                    ? Math.Round((alloc.ApprovedAmount - alloc.RemainingBalance) / alloc.ApprovedAmount * 100m, 2)
                    : null;
            }
        }
        catch { /* swallow budget failures */ }
    }

    // ---------------- Outstanding panel (invoiced-only) ----------------
    private async Task BuildOutstandingAsync(int poId, PoSummaryDto summary, CancellationToken ct)
    {
        try { summary.Outstanding.InvoicedAmount = await _repo.GetInvoicedTotalAsync(poId, ct); }
        catch { /* swallow */ }
    }

    private static string MapMethodToTransactionType(string? methodCode) => methodCode switch
    {
        MiscEnumEntity.Local => MiscEnumEntity.TransactionTypeLPO,
        MiscEnumEntity.Import => MiscEnumEntity.TransactionTypeIPO,
        MiscEnumEntity.Contract => MiscEnumEntity.TransactionTypeCPO,
        // Service / Blanket / Emergency fall back to LPO — if no matching workflow rows exist,
        // the approver name is simply omitted and the panel still shows the header status.
        _ => MiscEnumEntity.TransactionTypeLPO
    };
}
