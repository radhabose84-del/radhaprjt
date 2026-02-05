using AutoMapper;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using MediatR;

namespace PartyManagement.Application.BankMaster.Queries.GetBankMasterById;

public class GetBankMasterByIdHandler : IRequestHandler<GetBankMasterByIdQuery, BankMasterDto?>
{
    private readonly IBankMasterQueryRepository _qry;
    private readonly IMapper _mapper;
    public GetBankMasterByIdHandler(IBankMasterQueryRepository qry, IMapper mapper) => (_qry, _mapper) = (qry, mapper);

    public async Task<BankMasterDto?> Handle(GetBankMasterByIdQuery r, CancellationToken ct)
    {
        var e = await _qry.GetByIdAsync(r.Id, ct);
        return e is null ? null : _mapper.Map<BankMasterDto>(e);
    }
}
