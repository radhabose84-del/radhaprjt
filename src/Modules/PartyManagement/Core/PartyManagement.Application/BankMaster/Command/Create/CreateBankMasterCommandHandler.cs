using AutoMapper;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.BankMaster.Command.Create;

public class CreateBankMasterHandler : IRequestHandler<CreateBankMasterCommand, int>
{
    private readonly IBankMasterCommandRepository _cmd;
    private readonly IBankMasterQueryRepository _qry;
    private readonly IMapper _mapper;
    private readonly IIPAddressService _ip;
    private readonly IMediator _mediator;

    public CreateBankMasterHandler(IBankMasterCommandRepository cmd, IBankMasterQueryRepository qry, IMapper mapper, IIPAddressService ip,IMediator mediator)
        => (_cmd, _qry, _mapper, _ip,_mediator) = (cmd, qry, mapper, ip,mediator);

    public async Task<int> Handle(CreateBankMasterCommand r, CancellationToken ct)
    {
        var entity = _mapper.Map<PartyManagement.Domain.Entities.BankMaster>(r.Dto);
        entity.BankCode = await _qry.GenerateBankCodeAsync(entity.BankName, ct);

        var id = await _cmd.AddAsync(entity, ct);
        // AUDIT LOG event
        var audit = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: entity.BankCode,
                actionName: "Bank Master",
                details: $"Bank Master '{entity.BankCode}' (Tariff {entity.BankName}) was created.",
                module: "BankMaster"
            );
        await _mediator.Publish(audit, ct);
        return id;
        
    }
}
