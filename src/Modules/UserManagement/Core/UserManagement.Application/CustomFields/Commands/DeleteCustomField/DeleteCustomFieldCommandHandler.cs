using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.CustomFields.Commands.DeleteCustomField
{
    public class DeleteCustomFieldCommandHandler : IRequestHandler<DeleteCustomFieldCommand, bool>
    {
        private readonly ICustomFieldCommand _customFieldCommand;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        public DeleteCustomFieldCommandHandler(ICustomFieldCommand customFieldCommand, IMapper imapper, IMediator mediator)
        {
            _customFieldCommand = customFieldCommand;
            _imapper = imapper;
            _mediator = mediator;
        }
        public async Task<bool> Handle(DeleteCustomFieldCommand request, CancellationToken cancellationToken)
        {
            var customField  = _imapper.Map<CustomField>(request);
            var customFieldresult = await _customFieldCommand.DeleteAsync(request.Id, customField);


                  //Domain Event  
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Delete",
                        actionCode: "Delete custom field",
                        actionName: "Delete custom field",
                        details: $"custom field '{request.Id}' was deleted.",
                        module:"custom field"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken);  

                 if(customFieldresult)
                {
                    return customFieldresult;
                }
                throw new Exception("Custom field not deleted.");
              
        }
    }
}