using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.ICurrency
{
    public interface ICurrencyCommandRepository
    {
      Task<int> CreateAsync(UserManagement.Domain.Entities.Currency currency);
      Task<bool> ExistsByCodeAsync(string code ); // Check if code exists
      Task<bool> ExistsByNameupdateAsync(string name,int id );
      Task<int> UpdateAsync(int Id,UserManagement.Domain.Entities.Currency currency);
      Task<int> DeletecurrencyAsync(int Id,UserManagement.Domain.Entities.Currency currency);
      
        
    }

}
