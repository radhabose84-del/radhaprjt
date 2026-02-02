using PurchaseManagement.Application.ExchangeRate.Interfaces;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.ExchangeRate;

public sealed class ExchangeRateCommandRepository : IExchangeRateCommandRepository
{
    private readonly ApplicationDbContext _db;
    public ExchangeRateCommandRepository(ApplicationDbContext db) => _db = db;

    public async Task<int> UpsertAsync(IEnumerable<PurchaseManagement.Domain.Entities.ExchangeRate> items, CancellationToken ct)
    {
        int count = 0;

        foreach (var item in items)
        {
            // compute inverted/display rate safely
            item.ActualRate = item.Rate == 0m ? null : Math.Round(1m / item.Rate, 8, MidpointRounding.AwayFromZero);

            var existing = await _db.ExchangeRates.FirstOrDefaultAsync(x =>
                x.BaseCurrency == item.BaseCurrency &&
                x.QuoteCurrency == item.QuoteCurrency &&
                x.EffectiveDate == item.EffectiveDate, ct);

            if (existing is null)
            {
                _db.ExchangeRates.Add(item);
            }
            else
            {
                existing.Rate        = item.Rate;
                existing.ActualRate  = item.ActualRate;  // <— save inverted too
                existing.IsActive    = true;
                existing.Source      = item.Source;
                existing.ModifiedOnUtc = DateTime.UtcNow;
            }

            count++;
        }

        await _db.SaveChangesAsync(ct);
        return count;
    }
}
