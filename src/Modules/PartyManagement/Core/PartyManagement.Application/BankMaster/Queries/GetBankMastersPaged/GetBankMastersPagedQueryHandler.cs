using AutoMapper;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using MediatR;

namespace PartyManagement.Application.BankMaster.Queries.GetBankMastersPaged;

public class GetBankMastersPagedQueryHandler
    : IRequestHandler<GetBankMastersPagedQuery, (IReadOnlyList<BankMasterDto> Items, int Total)>
{
    private readonly IBankMasterQueryRepository _qry;
    private readonly IMapper _mapper;
    public GetBankMastersPagedQueryHandler(IBankMasterQueryRepository qry, IMapper mapper) => (_qry, _mapper) = (qry, mapper);

    public async Task<(IReadOnlyList<BankMasterDto> Items, int Total)> Handle(GetBankMastersPagedQuery r, CancellationToken ct)
    {
        var (entities, total) = await _qry.GetAllAsync(r.PageNumber, r.PageSize, r.Search, ct);
        return (entities.Select(_mapper.Map<BankMasterDto>).ToList(), total);
    }
}
