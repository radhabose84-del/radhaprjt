using System.Threading;
using System.Threading.Tasks;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using AutoMapper;

namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemById
{
    public sealed class GetItemByIdQueryHandler : IRequestHandler<GetItemByIdQuery, ItemDetailsDto?>
    {
        private readonly IItemQueryRepository _repo;
        public GetItemByIdQueryHandler(IItemQueryRepository repo) => _repo = repo;

        public Task<ItemDetailsDto?> Handle(GetItemByIdQuery request, CancellationToken ct)
            => _repo.GetByIdAsync(request.Id, ct);
    }
}
