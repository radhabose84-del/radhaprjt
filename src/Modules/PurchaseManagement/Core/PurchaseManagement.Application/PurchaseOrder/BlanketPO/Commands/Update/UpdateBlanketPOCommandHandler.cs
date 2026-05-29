using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBlanketPO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BlanketPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Update;

public sealed class UpdateBlanketPOCommandHandler
    : IRequestHandler<UpdateBlanketPOCommand, bool>
{
    private readonly IBlanketPOCommandRepository _commandRepo;
    private readonly IBlanketPOQueryRepository _releaseQueryRepo;
    private readonly IBlanketMasterQueryRepository _masterQueryRepo;
    private readonly IMediator _mediator;
    private readonly IIPAddressService _ipAddressService;
    private readonly IBudgetAllocationLookup _budgetAllocationLookup;
    private readonly IMiscMasterQueryRepository _misc;

    public UpdateBlanketPOCommandHandler(
        IBlanketPOCommandRepository commandRepo,
        IBlanketPOQueryRepository releaseQueryRepo,
        IBlanketMasterQueryRepository masterQueryRepo,
        IMediator mediator,
        IIPAddressService ipAddressService,
        IBudgetAllocationLookup budgetAllocationLookup,
        IMiscMasterQueryRepository misc)
    {
        _commandRepo = commandRepo;
        _releaseQueryRepo = releaseQueryRepo;
        _masterQueryRepo = masterQueryRepo;
        _mediator = mediator;
        _ipAddressService = ipAddressService;
        _budgetAllocationLookup = budgetAllocationLookup;
        _misc = misc;
    }

    public async Task<bool> Handle(UpdateBlanketPOCommand request, CancellationToken ct)
    {
        var r = request.Data;

        // Fetch blanket master
        var blanket = await _masterQueryRepo.GetByIdAsync(r.BlanketHeaderId, ct)
            ?? throw new ExceptionRules("Blanket Master not found.");

        // Validate balance for each detail (exclude self quantities)
        var balanceErrors = new List<string>();
        foreach (var item in r.Details)
        {
            var balance = await _releaseQueryRepo.GetBlanketDetailBalanceAsync(item.BlanketDetailId);
            // Add back existing quantity for this PO so we don't double-count
            if (item.Quantity > balance)
            {
                balanceErrors.Add(
                    $"Item {item.ItemSno}: Release quantity ({item.Quantity}) exceeds available blanket balance ({balance}) for Blanket Detail Id {item.BlanketDetailId}.");
            }
        }
        if (balanceErrors.Count > 0)
            throw new ExceptionRules(string.Join(" ", balanceErrors));

        var unitId = _ipAddressService.GetUnitId()
            ?? throw new ExceptionRules("UnitId is not available for the current user.");

        // Build PurchaseOrderHeader
        var poHeader = new PurchaseOrderHeader
        {
            Id = r.Id,
            UnitId = unitId,
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
            StatusId = r.StatusId,
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

        var updatedId = await _commandRepo.UpdateBlanketPOAsync(
            poHeader, blanketHeader, blanketDetails, paymentTerms, ct);

        // Audit
        var ev = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: "BLANKET_RELEASE_PO_UPDATE",
            actionName: r.Id.ToString(),
            details: $"Blanket Release PO with Id {r.Id} updated successfully.",
            module: "BlanketPO"
        );
        await _mediator.Publish(ev, ct);

        return updatedId > 0;
    }
}
