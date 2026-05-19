using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.ContractPO;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Create;

public sealed class CreateContractReleasePOCommandHandler
    : IRequestHandler<CreateContractReleasePOCommand, ApiResponseDTO<int>>
{
    private readonly IContractPOCommandRepository _commandRepo;
    private readonly IContractPOQueryRepository _queryRepo;
    private readonly IMediator _mediator;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;
    private readonly IIPAddressService _ipAddressService;

    public CreateContractReleasePOCommandHandler(
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

    public async Task<ApiResponseDTO<int>> Handle(
        CreateContractReleasePOCommand request, CancellationToken ct)
    {
        var r = request.Data;

        // Fetch contract to get VendorId, CurrencyId
        var contract = await _queryRepo.GetByIdAsync(r.ContractPOHeaderId, ct)
            ?? throw new ExceptionRules("Contract PO not found.");

        // Generate PONumber from DocumentSequence
        var unitId = _ipAddressService.GetUnitId()
            ?? throw new ExceptionRules("UnitId is not available for the current user.");
        var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
            MiscEnumEntity.TransactionTypeCPO, MiscEnumEntity.ModulePurchase, unitId)
            ?? throw new ExceptionRules("No transaction type configured for Contract PO.");

        var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
        var poNumber = sequences.Count > 0
            ? sequences[^1]
            : throw new ExceptionRules("No document sequence configured for Contract PO.");

        // Build PurchaseOrderHeader
        var poHeader = new PurchaseOrderHeader
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
            StatusId = r.StatusId
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

        // Persist in transaction (creates PO + contract header/details + release history + updates balances)
        var newPOId = await _commandRepo.CreateCombinePOAsync(
            poHeader, contractHeader, contractDetails, releaseHistories, transactionTypeId, ct);

        // Audit
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
}
