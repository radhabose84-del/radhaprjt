using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.ICity
{
    public interface ICityQueryRepository
    {
        Task<Cities> GetByIdAsync(int cityId);
        Task<(List<Cities>, int)> GetAllCityAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<Cities>> GetByCityNameAsync(string cityName);
        Task<List<Cities>> GetCityByStateIdAsync(int stateId);
        Task<bool> SoftDeleteValidation(int Id);
        Task<bool> IsCityLinkedAsync(int cityId);        
    }
}