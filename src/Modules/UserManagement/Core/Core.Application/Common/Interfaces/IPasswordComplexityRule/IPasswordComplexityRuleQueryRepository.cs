using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IPasswordComplexityRule
{
    public interface IPasswordComplexityRuleQueryRepository 
    {
        
     
       Task<(List<Core.Domain.Entities.PasswordComplexityRule>,int)> GetPasswordComplexityAsync(int PageNumber, int PageSize, string? SearchTerm);
      Task<Core.Domain.Entities.PasswordComplexityRule> GetByIdAsync(int id);  

      Task<List<Core.Domain.Entities.PasswordComplexityRule>> GetpwdautocompleteAsync(string searchTerm); 


      

    }
}