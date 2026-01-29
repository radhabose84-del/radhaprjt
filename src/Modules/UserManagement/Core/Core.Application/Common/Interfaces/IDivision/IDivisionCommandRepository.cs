using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using System.Text;

namespace Core.Application.Common.Interfaces.IDivision
{
    public interface IDivisionCommandRepository
    {  
        Task<Division> CreateAsync(Division division);     
        Task<bool> UpdateAsync(Division division);
        Task<bool> DeleteAsync(int id,Division division);        
    }
}