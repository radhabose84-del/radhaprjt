using PurchaseManagement.Application.ExchangeRate.Commands;
using MediatR;

namespace PurchaseManagement.Application.ExchangeRate.Queries.GetLatestRate;

public sealed record GetLatestRateQuery(string BaseCurrency, string QuoteCurrency) : IRequest<ExchangeRateDto?>;
