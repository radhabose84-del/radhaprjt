using Contracts.Common;
using Contracts.Interfaces;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using MediatR;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.Application.BankMaster.Command.Delete;
public class DeleteBankMasterCommandHandler : IRequestHandler<DeleteBankMasterCommand>
{
    private readonly IBankMasterQueryRepository _qry;
    private readonly IBankMasterCommandRepository _cmd;
    private readonly IIPAddressService _ip;

    public DeleteBankMasterCommandHandler(IBankMasterQueryRepository qry, IBankMasterCommandRepository cmd,IIPAddressService ip)
        => (_qry, _cmd,_ip) = (qry, cmd,ip);

    public async Task Handle(DeleteBankMasterCommand r, CancellationToken ct)
    {
        var e = await _qry.GetByIdAsync(r.Id, ct) ?? throw new KeyNotFoundException("Bank not found.");

        var isLinked = await _qry.SoftDeleteValidationAsync(r.Id);
        if (isLinked)
            throw new ExceptionRules("This master is linked with other records. You cannot delete this record.");
        e.IsDeleted = IsDelete.Deleted;
        e.ModifiedBy =_ip.GetUserId();
        e.ModifiedByName =_ip.GetUserName();
        e.ModifiedIP = _ip.GetUserIPAddress();
        e.ModifiedDate = DateTimeOffset.UtcNow;

        await _cmd.SoftDeleteAsync(e, ct);        
    }
}