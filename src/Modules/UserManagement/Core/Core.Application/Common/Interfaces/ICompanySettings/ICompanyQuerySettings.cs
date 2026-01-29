using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.ICompanySettings
{
    public interface ICompanyQuerySettings
    {
        Task<Core.Domain.Entities.CompanySettings> GetAsync();
        Task<bool> AlreadyExistsAsync(int CompanyId, int? id = null);
        Task<dynamic> BeforeLoginGetUserCompanySettings(string Username);
        Task<bool> BeforeLoginNotFoundValidation(string Username);
    }
}