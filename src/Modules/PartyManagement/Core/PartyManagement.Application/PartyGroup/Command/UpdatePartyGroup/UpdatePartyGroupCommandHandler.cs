using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PartyManagement.Application.Common.Exceptions;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.PartyGroup.Command.UpdatePartyGroup
{
    public class UpdatePartyGroupCommandHandler : IRequestHandler<UpdatePartyGroupCommand, bool>
    {
        private readonly IPartyGroupCommandRepository _ipartygroupcommandrepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public UpdatePartyGroupCommandHandler(IPartyGroupCommandRepository ipartygroupcommandrepository, IMediator imediator, IMapper imapper)
        {
            _ipartygroupcommandrepository = ipartygroupcommandrepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<bool> Handle(UpdatePartyGroupCommand request, CancellationToken cancellationToken)
        {
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