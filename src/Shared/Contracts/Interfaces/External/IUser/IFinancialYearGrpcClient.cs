using Contracts.Dtos.Users;

namespace Contracts.Interfaces.External.IUser
{
    public interface IFinancialYearGrpcClient
    {
        
            Task<List<FinancialYearDto>> GetAllFinancialYearAsync(CancellationToken ct = default);
            Task<List<FinancialYearDto>> GetByFinancialYearNameAsync(string searchPattern, CancellationToken ct = default);
            Task<FinancialYearDto> GetByIdsAsync(int id, CancellationToken ct = default);
    }
}