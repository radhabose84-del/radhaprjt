using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Domain.Entities.ContractPO;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Amendment;

public sealed class ContractReleasePOAmendmentCommandHandler
    : IRequestHandler<ContractReleasePOAmendmentCommand, int>
{
    private readonly IContractPOCommandRepository _commandRepo;
    private readonly IContractPOQueryRepository _queryRepo;
    private readonly IMediator _mediator;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;
    private readonly IIPAddressService _ipAddressService;

    public ContractReleasePOAmendmentCommandHandler(
        IContractPOCommandRepository commandRepo,
        IContractPOQueryRepository queryRepo,
        IMediator mediator,
        IDocumentSequenceLookup documentSequenceLookup,
        IIPAddressService ipAddressService)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mediator = mediator;
        _documentSequenceLookup = documentSequenceLookup;
        _ipAddressService = ipAddressService;
    }

    public async Task<int> Handle(ContractReleasePOAmendmentCommand request, CancellationToken ct)
    {
        var r = request.Data;

        // Get existing PO to verify and get current revision
        var existing = await _queryRepo.GetContractReleasePOByIdAsync(r.Id, ct)
            ?? throw new ExceptionRules("Contract Release PO not found.");

        // Fetch contract to get VendorId, CurrencyId
        var contract = await _queryRepo.GetByIdAsync(r.ContractPOHeaderId, ct)
            ?? throw new ExceptionRules("Contract PO not found.");

        // Generate new PONumber
        var unitId = _ipAddressService.GetUnitId()
            ?? throw new ExceptionRules("UnitId is not available for the current user.");
        var transactionTypeId = await _documentSequenceLookup
            .GetTransactionTypeIdAsync("CombinePO", "Purchase", unitId)
            ?? throw new ExceptionRules("No transaction type configured for Combine PO.");

        var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
        var poNumber = sequences.Count > 0
            ? sequences[^1]
            : throw new ExceptionRules("No document sequence configured for Combine PO.");

        var newRevisionNo = existing.RevisionNo + 1;

        // Build new PurchaseOrderHeader
        var newPoHeader = new PurchaseOrderHeader
        {
            UnitId = r.UnitId,
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
            StatusId = r.StatusId,
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

        // Amend: reverse old utilization, soft-close old PO, create new PO, apply new utilization
        var newPOId = await _commandRepo.AmendContractReleasePOAsync(
            r.Id, newPoHeader, newContractHeader, newContractDetails, newReleaseHistories,
            transactionTypeId, ct);

        // Audit
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
}
