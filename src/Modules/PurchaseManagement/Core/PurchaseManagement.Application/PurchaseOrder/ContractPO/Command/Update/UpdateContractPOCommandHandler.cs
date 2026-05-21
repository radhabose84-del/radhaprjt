using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
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
    private readonly IContractPOMasterQueryRepository _queryRepo;
    private readonly IMediator _mediator;
    private readonly IIPAddressService _ipAddressService;
    private readonly IMiscMasterQueryRepository _misc;

    public UpdateContractPOCommandHandler(
        IContractPOCommandRepository commandRepo,
        IContractPOMasterQueryRepository queryRepo,
        IMediator mediator,
        IIPAddressService ipAddressService,
        IMiscMasterQueryRepository misc)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mediator = mediator;
        _ipAddressService = ipAddressService;
        _misc = misc;
    }

    public async Task<bool> Handle(UpdateContractPOCommand request, CancellationToken ct)
    {
        var r = request.Data;

        // Fetch contract to get VendorId, CurrencyId
        var contract = await _queryRepo.GetByIdAsync(r.ContractPOHeaderId, ct)
            ?? throw new ExceptionRules("Contract PO not found.");

        // Ensure the contract is approved before allowing release PO update
        if (contract.StatusName != MiscEnumEntity.Approved)
            throw new ExceptionRules("This contract is not yet approved. Cannot update Release PO.");

        // Resolve UnitId from logged-in user and Pending status from MiscMaster
        var unitId = _ipAddressService.GetUnitId()
            ?? throw new ExceptionRules("UnitId is not available for the current user.");
        var pendingStatus = await _misc.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
        var pendingStatusId = pendingStatus.Id;

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
            StatusId = pendingStatusId
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
            poHeader, contractHeader, contractDetails, releaseHistories, ct);

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
