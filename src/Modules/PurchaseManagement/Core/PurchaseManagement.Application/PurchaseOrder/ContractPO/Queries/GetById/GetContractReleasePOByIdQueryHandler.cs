using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Queries.GetById;

public sealed class GetContractReleasePOByIdQueryHandler
    : IRequestHandler<GetContractReleasePOByIdQuery, ContractReleasePODetailVm?>
{
    private readonly IContractPOQueryRepository _queryRepo;

    public GetContractReleasePOByIdQueryHandler(IContractPOQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
    }

    public async Task<ContractReleasePODetailVm?> Handle(
        GetContractReleasePOByIdQuery request, CancellationToken ct)
    {
        return await _queryRepo.GetContractReleasePOByIdAsync(request.Id, ct);
    }
}
