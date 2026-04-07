using AutoMapper;
using Contracts.Common;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.PartyGroup.Command.UpdatePartyGroup
{
    public class UpdatePartyGroupCommandHandler : IRequestHandler<UpdatePartyGroupCommand, bool>
    {
        private readonly IPartyGroupCommandRepository _ipartygroupcommandrepository;
        private readonly IPartyGroupQueryRepository _ipartygroupqueryrepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public UpdatePartyGroupCommandHandler(IPartyGroupCommandRepository ipartygroupcommandrepository, IPartyGroupQueryRepository ipartygroupqueryrepository, IMediator imediator, IMapper imapper)
        {
            _ipartygroupcommandrepository = ipartygroupcommandrepository;
            _ipartygroupqueryrepository = ipartygroupqueryrepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<bool> Handle(UpdatePartyGroupCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _ipartygroupqueryrepository.IsPartyGroupLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules("This master is linked with other records. You cannot inactivate this record.");
            }

            var machineMaster = _imapper.Map<PartyManagement.Domain.Entities.PartyGroup>(request);
            var result = await _ipartygroupcommandrepository.UpdateAsync(request.Id, machineMaster);
          
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: machineMaster.Id.ToString(),
                actionName: machineMaster.PartyGroupName ?? "NULL",
                details: $"PartyGroup details was updated",
                module: "PartyGroup");
            await _imediator.Publish(domainEvent, cancellationToken);
           
            return result == true ? result : throw new ExceptionRules("PartyGroup Updation Failed.");
        }
    }
}