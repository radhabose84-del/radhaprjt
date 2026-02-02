
namespace PurchaseManagement.Application.Common.Interfaces.IExchangeRate;

public interface IExchangeRateQueryRepository
{
    Task<PurchaseManagement.Domain.Entities.ExchangeRate?> GetLatestAsync(string baseCcy, string quoteCcy, CancellationToken ct);    
    Task<PurchaseManagement.Domain.Entities.ExchangeRate?> GetByDateAsync(string baseCcy, string quoteCcy, DateOnly date, CancellationToken ct);    
}
