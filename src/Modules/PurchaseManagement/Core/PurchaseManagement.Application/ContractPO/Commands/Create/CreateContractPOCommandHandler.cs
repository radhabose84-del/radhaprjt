using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Application.ContractPO.Dto;
using PurchaseManagement.Domain.Entities.ContractPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.ContractPO.Commands.Create;

public sealed class CreateContractPOCommandHandler
    : IRequestHandler<CreateContractPOCommand, ContractPOHeaderDto>
{
    private readonly IContractPOCommandRepository _commandRepo;
    private readonly IContractPOQueryRepository _queryRepo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;
    private readonly IIPAddressService _ipAddressService;

    public CreateContractPOCommandHandler(
        IContractPOCommandRepository commandRepo,
        IContractPOQueryRepository queryRepo,
        IMapper mapper,
        IMediator mediator,
        IDocumentSequenceLookup documentSequenceLookup,
        IIPAddressService ipAddressService)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mapper = mapper;
        _mediator = mediator;
        _documentSequenceLookup = documentSequenceLookup;
        _ipAddressService = ipAddressService;
    }

    public async Task<ContractPOHeaderDto> Handle(
        CreateContractPOCommand request, CancellationToken ct)
    {
        // Map header
        var header = _mapper.Map<ContractPOHeader>(request);

        // Map and calculate detail values
        foreach (var detailItem in request.Details)
        {
            var detail = _mapper.Map<ContractPODetail>(detailItem);
            detail.ContractValue = detail.ContractQuantity * detail.ContractRate;
            detail.UtilizedQuantity = 0;
            detail.BalanceQuantity = detail.ContractQuantity;
            detail.UtilizedValue = 0;
            detail.BalanceValue = detail.ContractValue;
            header.ContractPODetails.Add(detail);
        }

        // Calculate header totals
        header.TotalContractValue = header.ContractPODetails.Sum(d => d.ContractValue);
        header.UtilizedValue = 0;
        header.BalanceValue = header.TotalContractValue;

        // Generate ContractPONumber from DocumentSequence
        var unitId = _ipAddressService.GetUnitId()
            ?? throw new ExceptionRules("UnitId is not available for the current user.");
        var transactionTypeId = await _documentSequenceLookup
            .GetTransactionTypeIdAsync("ContractPO", "Purchase", unitId)
            ?? throw new ExceptionRules("No transaction type configured for Contract PO.");

        var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
        header.ContractPONumber = sequences.Count > 0
            ? sequences[^1]
            : throw new ExceptionRules("No document sequence configured for Contract PO.");

        // Persist (IncrementDocNoAsync called inside repo transaction)
        var created = await _commandRepo.CreateAsync(header, transactionTypeId, ct);

        // Fetch fresh record with lookups
        var dto = await _queryRepo.GetByIdAsync(created.Id, ct)
                  ?? throw new ExceptionRules("Failed to read newly created Contract PO.");

        // Audit
        var ev = new AuditLogsDomainEvent(
            actionDetail: "Create",
            actionCode: dto.ContractPONumber ?? created.Id.ToString(),
            actionName: $"Contract PO for Vendor {dto.VendorId}",
            details: $"Contract PO '{dto.ContractPONumber}' created successfully with Id {created.Id}.",
            module: "ContractPO"
        );
        await _mediator.Publish(ev, ct);

        return dto;
    }
}
