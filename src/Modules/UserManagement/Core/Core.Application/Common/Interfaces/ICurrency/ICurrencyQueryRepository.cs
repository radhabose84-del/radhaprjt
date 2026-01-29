using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.ICurrency
{
  public interface ICurrencyQueryRepository
  {

    Task<(List<Core.Domain.Entities.Currency>, int)> GetAllCurrencyAsync(int PageNumber, int PageSize, string? SearchTerm);
    Task<Core.Domain.Entities.Currency?> GetByIdAsync(int Id);
    Task<List<Core.Domain.Entities.Currency>> GetByCurrencyNameAsync(string currency);
      Task<List<Domain.Entities.Currency>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}