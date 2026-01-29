using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.ITimeZones
{
    public interface ITimeZonesCommandRepository
    {
      Task<int> CreateAsync(Core.Domain.Entities.TimeZones timeZones);
      Task<int> UpdateAsync(int Id,Core.Domain.Entities.TimeZones timeZones);
      Task<int> DeleteEntityAsync(int Id,Core.Domain.Entities.TimeZones timeZones);
    }
}