using MediatR;

namespace PurchaseManagement.Application.ExchangeRate.Commands;

public sealed record ExchangeRateCommand(string BaseCurrency, string[] Symbols) : IRequest<int>;
