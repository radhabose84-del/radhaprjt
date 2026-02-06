using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IState
{
    public interface IStateQueryRepository
    {
        Task<States> GetByIdAsync(int stateId);
        Task<(List<States>, int)> GetAllStatesAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<States>> GetByStateNameAsync(string stateName);
        Task<List<States>> GetStateByCountryIdAsync(int countryId);
        Task<List<States>> GetCityByStateAsync(int stateId);
        Task<bool> SoftDeleteValidation(int Id);     
        Task<bool> IsLinkedWithCitiesAsync(int stateId);
    }
}