namespace UserManagement.Application.Common.Interfaces.ICurrency
{
    public interface ICurrencyQueryRepository
  {

    Task<(List<UserManagement.Domain.Entities.Currency>, int)> GetAllCurrencyAsync(int PageNumber, int PageSize, string? SearchTerm);
    Task<UserManagement.Domain.Entities.Currency?> GetByIdAsync(int Id);
    Task<List<UserManagement.Domain.Entities.Currency>> GetByCurrencyNameAsync(string currency);
      Task<List<UserManagement.Domain.Entities.Currency>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}