using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.ICountry
{
    public interface ICountryQueryRepository
    {
        Task<Countries> GetByIdAsync(int countryId);
        Task<(List<Countries>, int)> GetAllCountriesAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<Countries>> GetByCountryNameAsync(string countryName);
        Task<List<Countries>> GetStateByCountryIdAsync(int countryId);
        Task<bool> SoftDeleteValidation(int Id);         
        Task<bool> IsLinkedWithStatesAsync(int countryId);
    }
}