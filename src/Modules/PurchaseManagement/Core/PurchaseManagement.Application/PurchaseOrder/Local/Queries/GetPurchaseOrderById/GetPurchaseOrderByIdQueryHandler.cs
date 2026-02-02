using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using MediatR;
using PurchaseLocalDetailDto.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderById;

public class GetPurchaseOrderByIdQueryHandler 
    : IRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDetailDto?>
{
    private readonly IPurchaseOrderQueryRepository _repo;
    public GetPurchaseOrderByIdQueryHandler(IPurchaseOrderQueryRepository repo) => _repo = repo;

    public Task<PurchaseOrderDetailDto?> Handle(GetPurchaseOrderByIdQuery r, CancellationToken ct)
        => _repo.GetByIdAsync(r.Id, ct);
}
