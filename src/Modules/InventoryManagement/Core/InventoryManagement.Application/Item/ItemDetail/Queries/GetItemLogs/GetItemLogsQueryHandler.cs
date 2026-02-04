using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogs;
using MediatR;

namespace InventoryManagement.Application.ItemLogs.Queries
{
    public sealed class GetItemLogsQueryHandler 
        : IRequestHandler<GetItemLogsQuery, (List<ItemLogDto> Items, int TotalCount)>
    {
        private readonly IItemLogQueryRepository _repo;
        public GetItemLogsQueryHandler(IItemLogQueryRepository repo) => _repo = repo;

        public Task<(List<ItemLogDto> Items, int TotalCount)> Handle(GetItemLogsQuery request, CancellationToken ct)
            => _repo.GetAllAsync(request.Filter, ct);
    }
}
