using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.ICustomField;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repositories.CustomFields
{
    public class CustomFieldCommand : ICustomFieldCommand
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public CustomFieldCommand(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<int> CreateAsync(CustomField customField)
        {
            await _applicationDbContext.CustomField.AddAsync(customField);
            await _applicationDbContext.SaveChangesAsync();
            return customField.Id;
        }

        public async Task<bool> DeleteAsync(int id, CustomField customField)
        {
            var existingCustomField = await _applicationDbContext.CustomField.FirstOrDefaultAsync(u => u.Id == id);
            if (existingCustomField != null)
            {
                existingCustomField.IsDeleted = customField.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync() >0;
            }
            return false; 
        }

        public async Task<bool> UpdateAsync(CustomField customField)
        {
             var existingCustomField = await _applicationDbContext.CustomField
                   .Include(cf => cf.CustomFieldMenu)
                   .Include(cf => cf.CustomFieldUnits)
                   .Include(cf => cf.CustomFieldOptionalValues)
                   .FirstOrDefaultAsync(u => u.Id == customField.Id);

               if (existingCustomField == null)
                   return false;

               
               _applicationDbContext.CustomFieldMenu.RemoveRange(
                   _applicationDbContext.CustomFieldMenu.Where(x => x.CustomFieldId == customField.Id));

               _applicationDbContext.CustomFieldUnit.RemoveRange(
                   _applicationDbContext.CustomFieldUnit.Where(x => x.CustomFieldId == customField.Id));

               _applicationDbContext.CustomFieldOptionalValue.RemoveRange(
                   _applicationDbContext.CustomFieldOptionalValue.Where(x => x.CustomFieldId == customField.Id));

               
               existingCustomField.LabelName = customField.LabelName;
               existingCustomField.DataTypeId = customField.DataTypeId;
               existingCustomField.Length = customField.Length;
               existingCustomField.LabelTypeId = customField.LabelTypeId;
               existingCustomField.IsRequired = customField.IsRequired;
               existingCustomField.IsActive = customField.IsActive;

               
               if (customField.CustomFieldMenu?.Any() == true)
                   await _applicationDbContext.CustomFieldMenu.AddRangeAsync(customField.CustomFieldMenu);

               if (customField.CustomFieldUnits?.Any() == true)
                   await _applicationDbContext.CustomFieldUnit.AddRangeAsync(customField.CustomFieldUnits);

               if (customField.CustomFieldOptionalValues?.Any() == true)
                   await _applicationDbContext.CustomFieldOptionalValue.AddRangeAsync(customField.CustomFieldOptionalValues);

               return await _applicationDbContext.SaveChangesAsync() > 0;
        }
    }
}