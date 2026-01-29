using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using Core.Application.Common.Interfaces.IState;
using Core.Domain.Enums.Common;

namespace UserManagement.Infrastructure.Repositories.State
{    
    public class StateCommandRepository : IStateCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;        

        public StateCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;            
        }
        public async Task<States> CreateAsync(States states)
        {
            await _applicationDbContext.States.AddAsync(states);
            await _applicationDbContext.SaveChangesAsync();
            return states;
        }

        public async Task<int> DeleteAsync(int id, States states)
        {
             var StateToDelete = await _applicationDbContext.States.FirstOrDefaultAsync(u => u.Id == id);
            if (StateToDelete != null)
            {
                StateToDelete.IsDeleted = states.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0; 
        }  

        public async Task<int> UpdateAsync(int id, States state)        
        {
            var existingState = await _applicationDbContext.States.FirstOrDefaultAsync(u => u.Id == id);             
    
            if (existingState != null)
            {
                existingState.StateName = state.StateName;
                existingState.StateCode = state.StateCode;                
                existingState.CountryId = state.CountryId;
                existingState.IsActive = state.IsActive;                
                _applicationDbContext.States.Update(existingState);
                return await _applicationDbContext.SaveChangesAsync();
            }
           return 0; 
        }
        public async Task<bool> CountryExistsAsync(int countryId)
        {       
            return await _applicationDbContext.Countries.AnyAsync(s => s.Id == countryId && s.IsActive == Enums.Status.Active);
        }
        public async Task<States> GetStateByCodeAsync(string stateName,string stateCode, int countryId)
        {
                return await _applicationDbContext.States
                    .FirstOrDefaultAsync(s => s.StateCode == stateCode 
                                  && s.StateName == stateName && s.CountryId == countryId)?? new States()  ;
        }
    }
}
