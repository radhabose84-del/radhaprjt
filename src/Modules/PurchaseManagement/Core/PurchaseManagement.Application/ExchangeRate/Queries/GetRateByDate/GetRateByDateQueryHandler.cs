using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IExchangeRate;
using PurchaseManagement.Application.ExchangeRate.Commands;
using MediatR;

namespace PurchaseManagement.Application.ExchangeRate.Queries.GetRateByDate;

public sealed class GetRateByDateQueryHandler : IRequestHandler<GetRateByDateQuery, ExchangeRateDto?>
{
    private readonly IExchangeRateQueryRepository _repo;
    private readonly IMapper _mapper;

    public GetRateByDateQueryHandler(IExchangeRateQueryRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<ExchangeRateDto?> Handle(GetRateByDateQuery request, CancellationToken ct)
    {
        var entity = await _repo.GetByDateAsync(request.BaseCurrency, request.QuoteCurrency, request.Date, ct);
        return entity is null ? null : _mapper.Map<ExchangeRateDto>(entity);
    }
}
