using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetPOById;
public class GetImportPOByIdQueryHandler(IImportPOQueryRepository repo)
  : IRequestHandler<GetImportPOByIdQuery, ImportPOFullVm?>
{
    public Task<ImportPOFullVm?> Handle(GetImportPOByIdQuery r, CancellationToken ct)
        => repo.GetByIdAsync(r.Id, ct);
}