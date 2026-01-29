using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.ICustomField
{
    public interface ICustomFieldCommand
    {
        Task<int> CreateAsync(CustomField customField);     
        Task<bool> UpdateAsync(CustomField customField);
        Task<bool> DeleteAsync(int id,CustomField customField); 
    }
}