using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;

namespace InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands
{
    public interface IItemVariantAttributeCommandRepository
    {
        Task UpsertAttributesAsync(int itemId, List<VariantAttributeDto> attrs, CancellationToken ct = default);
        Task<List<VariantAttributeDto>> GetForItemAsync(int itemId, CancellationToken ct = default);
        Task AddMissingTemplateOptionsAsync(int templateItemId,IEnumerable<VariantValueDto> options, CancellationToken ct = default);

        /// <summary>
        /// Given a list of SpecificationValueIds, returns a map of
        /// SpecificationValueId → (SpecificationMasterId, SpecificationValue name).
        /// Used to auto-provision variant attributes from payload spec value ids.
        /// </summary>
        Task<Dictionary<int, (int SpecMasterId, string? SpecValueName)>> GetSpecificationValueMapAsync(
            IEnumerable<int> specificationValueIds, CancellationToken ct = default);
    }
}