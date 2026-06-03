using UserManagement.Application.Common.Interfaces.IStation;
using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repositories.Station
{
    public class StationCommandRepository : IStationCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public StationCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(UserManagement.Domain.Entities.Station station)
        {
            await _applicationDbContext.Station.AddAsync(station);
            await _applicationDbContext.SaveChangesAsync();
            return station.Id;
        }

        public async Task<bool> UpdateAsync(int id, UserManagement.Domain.Entities.Station station)
        {
            var existingStation = await _applicationDbContext.Station
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existingStation != null)
            {
                existingStation.StationName = station.StationName;
                existingStation.Description = station.Description;
                existingStation.IsActive = station.IsActive;

                _applicationDbContext.Station.Update(existingStation);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(int id, UserManagement.Domain.Entities.Station station)
        {
            var existingStation = await _applicationDbContext.Station.FirstOrDefaultAsync(u => u.Id == id);
            if (existingStation != null)
            {
                existingStation.IsDeleted = station.IsDeleted;

                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}
