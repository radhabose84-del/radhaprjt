using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.ILanguage
{
    public interface ILanguageQuery
    {
        Task<(List<Core.Domain.Entities.Language>,int)> GetAllLanguageAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<Core.Domain.Entities.Language> GetByIdAsync(int id);
        Task<List<Core.Domain.Entities.Language>> GetLanguage(string searchPattern);
        Task<Core.Domain.Entities.Language?> GetByLanguagenameAsync(string name,int? id = null);
    }
}