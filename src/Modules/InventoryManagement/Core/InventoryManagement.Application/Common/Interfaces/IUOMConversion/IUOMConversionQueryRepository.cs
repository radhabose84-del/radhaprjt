using InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion;

namespace InventoryManagement.Application.Common.Interfaces.IUOMConversion
{
    public interface IUOMConversionQueryRepository
    {


        Task<(List<UOMConversionDto>, int)> GetAllUOMConversionAsync(int PageNumber, int PageSize, string? SearchTerm);

        Task<UOMConversionDto> GetByIdAsync(int id);
        Task<bool> AlreadyExistsAsync(int fromUOMId, int toUOMId, int? id = null);

        Task<decimal?> GetConversionFactorAsync(int fromUOMId, int toUOMId);
 
    }
}