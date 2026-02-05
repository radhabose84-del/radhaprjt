// PurchaseManagement.Application/ExchangeRate/ExchangeRateCommandHandler.cs
using AutoMapper;
using PurchaseManagement.Application.ExchangeRate.Interfaces;
using PurchaseManagement.Application.External;
using MediatR;

namespace PurchaseManagement.Application.ExchangeRate.Commands;

public sealed class ExchangeRateCommandHandler : IRequestHandler<ExchangeRateCommand, int>
{
    private readonly IFrankfurterClient _fx;
    private readonly IExchangeRateCommandRepository _cmdRepo;
    private readonly IMediator _mediator;   // for domain events if needed
    private readonly IMapper _mapper;       // reserved for DTO maps if needed

    public ExchangeRateCommandHandler(
        IFrankfurterClient fx,
        IExchangeRateCommandRepository cmdRepo,
        IMediator mediator,
        IMapper mapper)
    {
        _fx = fx;
        _cmdRepo = cmdRepo;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<int> Handle(ExchangeRateCommand request, CancellationToken ct)
    {
        var resp = await _fx.GetLatestAsync(request.BaseCurrency, request.Symbols, ct);
        var effectiveDate = DateOnly.Parse(resp.date);

        var entities = resp.rates.Select(kv => new PurchaseManagement.Domain.Entities.ExchangeRate
        {
            BaseCurrency = resp.@base,     // e.g., INR
            QuoteCurrency = kv.Key,        // USD, EUR
            Rate = kv.Value,
            EffectiveDate = effectiveDate,
            Source = "Frankfurter",
            IsActive = true
        });

        var upserted = await _cmdRepo.UpsertAsync(entities, ct);


        return upserted;
    }
}
