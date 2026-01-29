using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.ILanguage
{
    public interface ILanguageCommand
    {
         Task<Core.Domain.Entities.Language> CreateAsync(Core.Domain.Entities.Language language);     
         Task<bool> UpdateAsync(Core.Domain.Entities.Language language);
        Task<bool> DeleteAsync(int id,Core.Domain.Entities.Language language);   
    }
}