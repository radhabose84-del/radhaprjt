using PurchaseManagement.Application.ExchangeRate.Commands;
using MediatR;

namespace  PurchaseManagement.Application.ExchangeRate.Queries.GetRateByDate;
public sealed record GetRateByDateQuery(string BaseCurrency, string QuoteCurrency, DateOnly Date)
    : IRequest<ExchangeRateDto?>;
