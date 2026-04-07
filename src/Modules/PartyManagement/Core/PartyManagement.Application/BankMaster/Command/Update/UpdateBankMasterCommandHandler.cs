
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using MediatR;

namespace PartyManagement.Application.BankMaster.Command.Update;


public class UpdateBankMasterCommandHandler : IRequestHandler<UpdateBankMasterCommand>
{
    private readonly IBankMasterQueryRepository _qry;
    private readonly IBankMasterCommandRepository _cmd;
    private readonly IMapper _mapper;
    private readonly IIPAddressService _ip;

    public UpdateBankMasterCommandHandler(IBankMasterQueryRepository qry, IBankMasterCommandRepository cmd, IMapper mapper,IIPAddressService ip)
        => (_qry, _cmd, _mapper,_ip) = (qry, cmd, mapper,ip);

    public async Task Handle(UpdateBankMasterCommand r, CancellationToken ct)
    {
        if (r.Dto.IsActive == 0)
        {
            var isLinked = await _qry.IsBankMasterLinkedAsync(r.Dto.Id);
            if (isLinked)
                throw new ExceptionRules("This master is linked with other records. You cannot inactivate this record.");
        }

        var e = await _qry.GetByIdAsync(r.Dto.Id, ct) ?? throw new KeyNotFoundException("Bank not found.");

        if (await _qry.ExistsByBankCodeAsync(r.Dto.BankName, r.Dto.Id, ct))
            throw new InvalidOperationException("BankName already exists for another record.");

        _mapper.Map(r.Dto, e);
        e.ModifiedBy = _ip.GetUserId();
        e.ModifiedByName = _ip.GetUserName();
        e.ModifiedIP = _ip.GetUserIPAddress();
        e.ModifiedDate = DateTimeOffset.UtcNow;

        await _cmd.UpdateAsync(e, ct);        
    }
}
