using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.ICustomField
{
    public interface ICustomFieldCommand
    {
        Task<int> CreateAsync(CustomField customField);     
        Task<bool> UpdateAsync(CustomField customField);
        Task<bool> DeleteAsync(int id,CustomField customField); 
    }
}