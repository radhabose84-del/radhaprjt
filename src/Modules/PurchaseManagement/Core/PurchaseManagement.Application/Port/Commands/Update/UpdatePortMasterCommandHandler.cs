using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Application.Port.Dto;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.Port.Commands.Update
{
    public sealed class UpdatePortMasterCommandHandler
        : IRequestHandler<UpdatePortMasterCommand, PortMasterDto>
    {
        private readonly IPortMasterCommandRepository _commandRepo;
        private readonly IPortMasterQueryRepository _queryRepo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdatePortMasterCommandHandler(
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

        public async Task<PortMasterDto> Handle(UpdatePortMasterCommand request, CancellationToken ct)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepo.IsPortMasterLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            // map request -> entity (Id must be present)
            var toUpdate = _mapper.Map<PortMaster>(request);

            var updated = await _commandRepo.UpdateAsync(toUpdate, ct);

            // read back (Dapper)
            var fresh = await _queryRepo.GetByIdAsync(updated.Id, ct)
                        ?? throw new ExceptionRules("Updated record not found.");
            var result = _mapper.Map<PortMasterDto>(fresh);

            // audit/event
            var ev = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: result.PortCode,
                actionName: result.PortName,
                details: $"PortMaster '{result.PortCode} - {result.PortName}' updated.",
                module: "PortMaster"
            );
            await _mediator.Publish(ev, ct);

            return result;
        }
    }
}
