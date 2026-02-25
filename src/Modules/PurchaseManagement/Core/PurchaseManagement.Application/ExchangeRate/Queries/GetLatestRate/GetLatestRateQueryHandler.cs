// PurchaseManagement.Application/ExchangeRate/Queries/GetLatestRateQueryHandler.cs
using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IExchangeRate;
using PurchaseManagement.Application.ExchangeRate.Commands;
using MediatR;

namespace PurchaseManagement.Application.ExchangeRate.Queries.GetLatestRate;

public sealed class GetLatestRateQueryHandler : IRequestHandler<GetLatestRateQuery, ExchangeRateDto?>
{
    private readonly IExchangeRateQueryRepository _repo;
    private readonly IMapper _mapper;

    public GetLatestRateQueryHandler(IExchangeRateQueryRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<ExchangeRateDto?> Handle(GetLatestRateQuery request, CancellationToken ct)
    {
        var entity = await _repo.GetLatestAsync(request.BaseCurrency, request.QuoteCurrency, ct);
        return entity is null ? null : _mapper.Map<ExchangeRateDto>(entity);
    }
}
