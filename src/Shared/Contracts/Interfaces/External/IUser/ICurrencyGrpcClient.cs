using Contracts.Dtos.Users;

namespace Contracts.Interfaces.External.IUser
{
    public interface ICurrencyGrpcClient
    {
            Task<CurrencyDto?> GetByIdAsync(int id, CancellationToken ct = default);  
            Task<List<CurrencyDto>> GetByCurrencyNameAsync(string searchPattern, CancellationToken ct = default);
            Task<List<CurrencyDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
