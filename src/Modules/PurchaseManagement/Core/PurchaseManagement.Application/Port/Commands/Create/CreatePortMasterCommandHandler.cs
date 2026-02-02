using AutoMapper;
using PurchaseManagement.Application.Common.Exceptions;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Application.Port.Commands;
using PurchaseManagement.Application.Port.Dto;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.Port.Commands
{
    public sealed class CreatePortMasterCommandHandler
        : IRequestHandler<CreatePortMasterCommand, PortMasterDto>
    {
        private readonly IPortMasterCommandRepository _commandRepo;
        private readonly IPortMasterQueryRepository _queryRepo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreatePortMasterCommandHandler(
            IPortMasterCommandRepository commandRepo,
            IPortMasterQueryRepository queryRepo,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepo = commandRepo;
            _queryRepo   = queryRepo;
            _mapper      = mapper;
            _mediator    = mediator;
        }

        public async Task<PortMasterDto> Handle(CreatePortMasterCommand request, CancellationToken ct)
        {
            // map request -> entity
            var entity = _mapper.Map<PortMaster>(request);

            // write (EF)
            var created = await _commandRepo.CreateAsync(entity, ct);

            // read back (Dapper)
            var fresh = await _queryRepo.GetByIdAsync(created.Id, ct);
            var result = _mapper.Map<PortMasterDto>(fresh);

            // audit/event
            var ev = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: entity.PortCode,
                actionName: entity.PortName,
                details: $"PortMaster '{entity.PortCode} - {entity.PortName}' created.",
                module: "PortMaster"
            );
            await _mediator.Publish(ev, ct);

            return created.Id <= 0
                ? throw new ExceptionRules("Failed to create Port Master")
                : result!;
        }
    }
}
