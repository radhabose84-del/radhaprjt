using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.CreateItemTemplate
{
    public sealed class CreateItemTemplateCommandHandler : IRequestHandler<CreateItemTemplateCommand, int>
    {        
        private readonly IItemVariantAttributeCommandRepository _attrRepo;
        private readonly IItemQueryRepository _itemQry;
        private readonly IItemCommandRepository _itemRepo;        
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public async Task<int> Handle(CreateItemTemplateCommand request, CancellationToken ct)
        {
            var p = request.Payload;

            // validate (you can hook FluentValidation pipeline)
            if (!p.HasVariants) throw new InvalidOperationException("Template must have HasVariants=true.");

            var code = await _itemQry.GetLatestItemCode(p.ItemGroupId!.Value, p.ItemCategoryId!.Value, ct)
                       ?? throw new InvalidOperationException("Failed to generate ItemCode.");
            if (await _itemRepo.ExistsByCodeForCreateAsync(code, ct))
                throw new InvalidOperationException($"Generated ItemCode '{code}' already exists.");

            var id = await _uow.ExecuteInTransactionAsync<int>(async _ =>
            {
                var item = _mapper.Map<ItemMaster>(p);
                item.ItemCode = code;
                item.HasVariants = true;
                item.ParentItemId = null;
                item.IsActive = BaseEntity.Status.Active;

                var newId = await _itemRepo.CreateAsync(item, ct);

                // tabs/collections (optional from payload)
                // ... same as your current code ...

                // save attributes only
                await _attrRepo.UpsertAttributesAsync(newId, p.VariantAttributes, ct);

                return newId;
            }, ct);

            return id;
        }
    }
}