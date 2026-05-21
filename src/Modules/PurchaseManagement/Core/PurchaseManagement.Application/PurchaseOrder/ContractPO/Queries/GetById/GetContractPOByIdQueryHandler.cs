using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Queries.GetById;

public sealed class GetContractPOByIdQueryHandler
    : IRequestHandler<GetContractPOByIdQuery, ContractPODetailVm?>
{
    private readonly IContractPOQueryRepository _queryRepo;

    public GetContractPOByIdQueryHandler(IContractPOQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
    }

    public async Task<ContractPODetailVm?> Handle(
        GetContractPOByIdQuery request, CancellationToken ct)
    {
        return await _queryRepo.GetContractPOByIdAsync(request.Id, ct);
    }
}
