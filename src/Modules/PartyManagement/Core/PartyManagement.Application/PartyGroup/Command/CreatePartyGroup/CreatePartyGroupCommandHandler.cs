using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using PartyManagement.Application.Common.Exceptions;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.PartyGroup.Command.CreatePartyGroup
{
    public class CreatePartyGroupCommandHandler : IRequestHandler<CreatePartyGroupCommand, int>
    {
        private readonly IPartyGroupCommandRepository _ipartygroupcommandrepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public CreatePartyGroupCommandHandler(IPartyGroupCommandRepository ipartygroupcommandrepository, IMediator imediator, IMapper imapper)
        {
            _ipartygroupcommandrepository = ipartygroupcommandrepository;
            _imediator = imediator;
            _imapper = imapper;
        }
        public async Task<int> Handle(CreatePartyGroupCommand request, CancellationToken cancellationToken)
        {
            var partyGroupMaster = _imapper.Map<PartyManagement.Domain.Entities.PartyGroup>(request);
            
            var result = await _ipartygroupcommandrepository.CreateAsync(partyGroupMaster);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: partyGroupMaster.PartyGroupName?? "NULL",
                actionName: partyGroupMaster.ParentPartyGroupId.ToString()?? "NULL",
                details: $"PartyGroup details was created",
                module: "PartyGroup");
            await _imediator.Publish(domainEvent, cancellationToken);
         
            return result > 0 ? result : throw new ExceptionRules("PartyGroup Creation Failed.");
        }
    }
}