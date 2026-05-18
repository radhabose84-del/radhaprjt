using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IContractPO;
using PurchaseManagement.Application.ContractPO.Dto;
using PurchaseManagement.Domain.Entities.ContractPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.ContractPO.Commands.Update;

public sealed class UpdateContractPOCommandHandler
    : IRequestHandler<UpdateContractPOCommand, ContractPOHeaderDto>
{
    private readonly IContractPOCommandRepository _commandRepo;
    private readonly IContractPOQueryRepository _queryRepo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public UpdateContractPOCommandHandler(
        IContractPOCommandRepository commandRepo,
        IContractPOQueryRepository queryRepo,
        IMapper mapper,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<ContractPOHeaderDto> Handle(
        UpdateContractPOCommand request, CancellationToken ct)
    {
        // Map header from command
        var header = _mapper.Map<ContractPOHeader>(request);

        // Build detail list with calculated values
        var details = new List<ContractPODetail>();
        foreach (var item in request.Details)
        {
            var detail = new ContractPODetail
            {
                Id = item.Id,
                ItemSno = item.ItemSno,
                ItemId = item.ItemId,
                UOMId = item.UOMId,
                ContractQuantity = item.ContractQuantity,
                ContractRate = item.ContractRate,
                ContractValue = item.ContractQuantity * item.ContractRate,
                HSNId = item.HSNId,
                GSTPercentage = item.GSTPercentage
            };
            // Balance fields are recalculated in the command repository
            details.Add(detail);
        }

        // Persist
        var updated = await _commandRepo.UpdateAsync(header, details, ct);

        // Fetch fresh record
        var dto = await _queryRepo.GetByIdAsync(updated.Id, ct)
                  ?? throw new ExceptionRules("Updated Contract PO not found.");

        // Audit
        var ev = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: dto.ContractPONumber ?? updated.Id.ToString(),
            actionName: $"Contract PO {dto.ContractPONumber}",
            details: $"Contract PO '{dto.ContractPONumber}' with Id {updated.Id} updated successfully.",
            module: "ContractPO"
        );
        await _mediator.Publish(ev, ct);

        return dto;
    }
}
