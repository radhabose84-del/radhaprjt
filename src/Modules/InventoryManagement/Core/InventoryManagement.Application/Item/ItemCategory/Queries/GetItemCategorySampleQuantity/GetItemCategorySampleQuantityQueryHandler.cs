using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.Shared;
using MediatR;

namespace InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategorySampleQuantity
{
    public class GetItemCategorySampleQuantityQueryHandler : IRequestHandler<GetItemCategorySampleQuantityQuery, SampleQuantityDto?>
    {
        private readonly IItemCategoryQueryRepository _queryRepository;

        public GetItemCategorySampleQuantityQueryHandler(IItemCategoryQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<SampleQuantityDto?> Handle(GetItemCategorySampleQuantityQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.GetSampleQuantityAsync(request.ItemCategoryId, request.UnitId);
        }
    }
}
