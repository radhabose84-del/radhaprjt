using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.IState
{
    public interface IStateCommandRepository
    {
        Task<States> CreateAsync(States state);        
        Task<int>  UpdateAsync(int stateId,States state);
        Task<int>  DeleteAsync(int stateId,States state);             
        Task<bool> CountryExistsAsync(int stateId); 
        Task<States> GetStateByCodeAsync(string StateName,string StateCode, int countryId);         
    }
}