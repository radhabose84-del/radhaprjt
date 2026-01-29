using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.ITimeZones
{
    public interface ITimeZonesQueryRepository
    {
     Task<(List<Core.Domain.Entities.TimeZones>,int)> GetAllTimeZonesAsync(int PageNumber, int PageSize, string? SearchTerm);
      Task<Core.Domain.Entities.TimeZones> GetByIdAsync(int Id);
      Task<List<Core.Domain.Entities.TimeZones>> GetByTimeZonesNameAsync(string timeZones);


     
    }
}