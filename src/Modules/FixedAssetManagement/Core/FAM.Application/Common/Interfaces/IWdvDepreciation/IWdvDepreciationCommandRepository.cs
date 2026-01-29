using FAM.Application.WDVDepreciation.Queries.CalculateDepreciation;

namespace FAM.Application.Common.Interfaces.IWdvDepreciation
{
    public interface IWdvDepreciationCommandRepository
    {               
        Task<int> DeleteAsync(int finYearId);     
        Task<int> LockWDVDepreciationAsync(int finYearId);        
    }
}