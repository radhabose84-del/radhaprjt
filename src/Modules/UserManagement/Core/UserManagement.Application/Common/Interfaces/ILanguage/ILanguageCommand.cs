using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.ILanguage
{
    public interface ILanguageCommand
    {
         Task<UserManagement.Domain.Entities.Language> CreateAsync(UserManagement.Domain.Entities.Language language);     
         Task<bool> UpdateAsync(UserManagement.Domain.Entities.Language language);
        Task<bool> DeleteAsync(int id,UserManagement.Domain.Entities.Language language);   
    }
}