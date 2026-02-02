
namespace  PurchaseManagement.Application.ExchangeRate.Interfaces;
public interface IExchangeRateCommandRepository
{
    Task<int> UpsertAsync(IEnumerable<Domain.Entities.ExchangeRate> items, CancellationToken ct);
}
