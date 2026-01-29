using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.ICustomField
{
    public interface ICustomFieldQuery
    {
         Task<(List<CustomField>,int)> GetAllCustomFieldsAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<(dynamic CustomField,IList<dynamic> CustomFieldMenu,IList<dynamic> CustomFieldUnit,IList<dynamic> CustomFieldOptionValue)> GetByIdAsync(int id);
        Task<List<CustomField>> GetCustomField(string searchPattern);
        Task<bool> AlreadyExistsAsync(string LabelName, int? id = null);
        Task<bool> SoftDeleteValidation(int Id); 
        Task<bool> FKColumnExistValidation(int Id);
        Task<bool> NotFoundAsync(int id );
    }
}