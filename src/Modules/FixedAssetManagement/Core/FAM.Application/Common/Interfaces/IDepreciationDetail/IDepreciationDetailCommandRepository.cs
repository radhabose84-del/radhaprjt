using System;
using System.Threading.Tasks;
using FAM.Domain.Entities;

namespace FAM.Application.Common.Interfaces.IDepreciationDetail
{
    public interface IDepreciationDetailCommandRepository
    {        
        Task<int> DeleteAsync( int finYearId, int depreciationType,int depreciationPeriod);      
        Task<int> UpdateAsync(int finYearId, int depreciationType,int depreciationPeriod);   
        
    }
}
