
using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemGroup
{
    public class ItemGroupCommandRepository : IItemGroupCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IIPAddressService _ipAddressService;

        public ItemGroupCommandRepository(ApplicationDbContext applicationDbContext, IIPAddressService ipAddressService)
        {
            _applicationDbContext = applicationDbContext;
            _ipAddressService = ipAddressService;
        }

        public async Task<int> CreateAsync(InventoryManagement.Domain.Entities.Item.ItemGroup itemGroup)
        {
            var entry = _applicationDbContext.Entry(itemGroup);
            itemGroup.UnitId = _ipAddressService.GetUnitId() ?? 0;
            await _applicationDbContext.ItemGroup.AddAsync(itemGroup);
            await _applicationDbContext.SaveChangesAsync();
            return itemGroup.Id;
        }

        public async Task<int> DeleteAsync(int Id, InventoryManagement.Domain.Entities.Item.ItemGroup itemGroup)
        {
            var itemGroupToDelete = await _applicationDbContext.ItemGroup.FirstOrDefaultAsync(u => u.Id == Id);
            if (itemGroupToDelete is null)
            {
                return -1;
            }
            itemGroupToDelete.IsDeleted = itemGroup.IsDeleted;
            await _applicationDbContext.SaveChangesAsync();
            return 1;
        }
        public async Task<int> UpdateAsync(int Id, InventoryManagement.Domain.Entities.Item.ItemGroup itemGroup)
        {
            var existingItemGroup = await _applicationDbContext.ItemGroup.FirstOrDefaultAsync(u => u.Id == Id);
            if (existingItemGroup is null)
            {
                return -1;
            }
            existingItemGroup.ItemGroupName = itemGroup.ItemGroupName;
            existingItemGroup.ItemGroupCode = itemGroup.ItemGroupCode;
            existingItemGroup.IsActive = itemGroup.IsActive;

            _applicationDbContext.ItemGroup.Update(existingItemGroup);

            await _applicationDbContext.SaveChangesAsync();
            return 1;
        }
        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _applicationDbContext.ItemGroup
            .Where(cc => cc.ItemGroupCode == code && cc.IsDeleted == 0)
            .AnyAsync();
        }

        public async Task<bool> IsNameDuplicateAsync(string name, int excludeId)
        {
            return await _applicationDbContext.ItemGroup
            .Where(cc => cc.ItemGroupName == name && cc.Id != excludeId)
            .AnyAsync();
        }
        public async Task<bool> IsCodeDuplicateAsync(string? code, int excludeId)
        {
            return await _applicationDbContext.ItemGroup
            .AnyAsync(cc => cc.ItemGroupCode == code && cc.Id != excludeId && cc.IsDeleted == 0);
        }
        public async Task<bool> ExistsByNameAsync(string name,  CancellationToken ct = default)
        {
            name = (name ?? "").Trim();

            return await _applicationDbContext.ItemGroup
                .AsNoTracking()
                .AnyAsync(x =>
                    x.UnitId == (_ipAddressService.GetUnitId() ?? 0) &&
                     x.IsDeleted == IsDelete.NotDeleted && 
                    x.ItemGroupName != null &&
                    x.ItemGroupName.Trim() == name,
                    ct);
        }


    }
}