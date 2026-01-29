using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using Core.Application.Common.Interfaces.ICity;
using Core.Domain.Enums.Common;

namespace UserManagement.Infrastructure.Repositories.City
{
    public class CityCommandRepository : ICityCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;        
        public CityCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;            
        }
        public async Task<Cities> CreateAsync(Cities cities)
        {
            await _applicationDbContext.Cities.AddAsync(cities);
            await _applicationDbContext.SaveChangesAsync();
            return cities;
        }
        public async Task<int> DeleteAsync(int id, Cities cities)
        {
            var CityToDelete = await _applicationDbContext.Cities.FirstOrDefaultAsync(u => u.Id == id);
            if (CityToDelete != null)
            {
                CityToDelete.IsDeleted = cities.IsDeleted;              
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0; 
        }
        public async Task<int> UpdateAsync(int id, Cities city)
        {
            var existingCity = await _applicationDbContext.Cities.FirstOrDefaultAsync(u => u.Id == id);             
    
            if (existingCity != null)
            {
                existingCity.CityName = city.CityName;
                existingCity.CityCode = city.CityCode;                
                existingCity.StateId = city.StateId;
                existingCity.IsActive = city.IsActive;
                _applicationDbContext.Cities.Update(existingCity);
                return await _applicationDbContext.SaveChangesAsync();
            }
           return 0; 
        }
        public async Task<bool> StateExistsAsync(int stateId)
        {        
            return await _applicationDbContext.States.AnyAsync(s => s.Id == stateId  &&  s.IsActive == Enums.Status.Active && s.IsDeleted==0 );
        }
        public async Task<Cities> GetCityByNameAsync(string cityName,string cityCode, int stateId)
        {           
            return await  _applicationDbContext.Cities
                .FirstOrDefaultAsync(s => s.CityCode == cityCode 
                            && s.CityName == cityName && s.StateId == stateId && s.IsDeleted==0 ) ?? new Cities();                          
        }

    }
}