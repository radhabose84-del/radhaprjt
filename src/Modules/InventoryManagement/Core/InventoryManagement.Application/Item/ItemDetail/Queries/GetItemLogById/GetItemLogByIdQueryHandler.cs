using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogs;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById
{
    public sealed class GetItemLogByIdQueryHandler
        : IRequestHandler<GetItemLogByIdQuery, ItemLogDto?>
    {
        private readonly IItemLogQueryRepository _repo;
        public GetItemLogByIdQueryHandler(IItemLogQueryRepository repo) => _repo = repo;

        public Task<ItemLogDto?> Handle(GetItemLogByIdQuery request, CancellationToken ct)
            => _repo.GetByIdAsync(request.Id, ct);
    }
}
