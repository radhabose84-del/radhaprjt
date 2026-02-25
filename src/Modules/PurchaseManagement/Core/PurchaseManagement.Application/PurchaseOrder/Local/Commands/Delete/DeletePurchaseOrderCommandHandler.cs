using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Delete;
public class DeletePurchaseOrderCommandHandler : IRequestHandler<DeletePurchaseOrderCommand, bool>
{
    private readonly IPurchaseOrderCommandRepository _repo;
    public DeletePurchaseOrderCommandHandler(IPurchaseOrderCommandRepository repo) => _repo = repo;

    public async Task<bool> Handle(DeletePurchaseOrderCommand request, CancellationToken ct)
        => (await _repo.SoftDeleteAsync(request.Id, ct)) > 0;
}
