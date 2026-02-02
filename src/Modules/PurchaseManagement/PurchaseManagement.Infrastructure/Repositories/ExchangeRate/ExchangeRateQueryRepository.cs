using System.Data;
using Dapper;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Application.Common.Interfaces.IExchangeRate;

namespace PurchaseManagement.Infrastructure.Repositories;

public sealed class ExchangeRateQueryRepository : IExchangeRateQueryRepository
{
    private readonly IDbConnection _dbConnection;
    public ExchangeRateQueryRepository(IDbConnection dbConnection) => _dbConnection = dbConnection;

    public async Task<PurchaseManagement.Domain.Entities.ExchangeRate?> GetLatestAsync(string baseCcy, string quoteCcy, CancellationToken ct)
    {
        const string sql = @"
            SELECT TOP(1)
                Id, BaseCurrency, QuoteCurrency, Rate, EffectiveDate, Source, IsActive,
                CreatedOnUtc, ModifiedOnUtc,ActualRate
            FROM Purchase.ExchangeRate WITH (READCOMMITTED)
            WHERE BaseCurrency = @Base AND QuoteCurrency = @Quote
            ORDER BY EffectiveDate DESC, Id DESC;";

        var def = new CommandDefinition(sql, new { Base = baseCcy, Quote = quoteCcy }, cancellationToken: ct);
        return await _dbConnection.QueryFirstOrDefaultAsync<PurchaseManagement.Domain.Entities.ExchangeRate>(def);
    }

    public async Task<PurchaseManagement.Domain.Entities.ExchangeRate?> GetByDateAsync(string baseCcy, string quoteCcy, DateOnly date, CancellationToken ct)
    {
        const string sql = @"
            SELECT
                Id, BaseCurrency, QuoteCurrency, Rate, EffectiveDate, Source, IsActive,
                CreatedOnUtc, ModifiedOnUtc,ActualRate
            FROM Purchase.ExchangeRate WITH (READCOMMITTED)
            WHERE BaseCurrency = @Base AND QuoteCurrency = @Quote AND EffectiveDate = @Date;";

        var def = new CommandDefinition(sql, new { Base = baseCcy, Quote = quoteCcy, Date = date }, cancellationToken: ct);
        return await _dbConnection.QueryFirstOrDefaultAsync<PurchaseManagement.Domain.Entities.ExchangeRate>(def);
    }
}
