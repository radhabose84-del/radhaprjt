using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Common.Interfaces.ITimeZones
{
    public interface ITimeZonesQueryRepository
    {
     Task<(List<UserManagement.Domain.Entities.TimeZones>,int)> GetAllTimeZonesAsync(int PageNumber, int PageSize, string? SearchTerm);
      Task<UserManagement.Domain.Entities.TimeZones> GetByIdAsync(int Id);
      Task<List<UserManagement.Domain.Entities.TimeZones>> GetByTimeZonesNameAsync(string timeZones);


     
    }
}