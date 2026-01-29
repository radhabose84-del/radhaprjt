using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.ICountry
{
    public interface ICountryCommandRepository
    {
        Task<Countries> CreateAsync(Countries country);
        Task<int> UpdateAsync(int countryId, Countries country);
        Task<int> DeleteAsync(int countryId, Countries country);
        Task<Countries> GetCountryByCodeAsync(string countryName, string countryCode);     
        Task<bool> ExistsByCodeAsync(string code, int excludeId = 0, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, int excludeId = 0, CancellationToken ct = default);    
    }
}