using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.CustomFields.Commands.CreateCustomField
{
    public class CreateCustomFieldCommandhandler : IRequestHandler<CreateCustomFieldCommand, int>
    {
        private readonly ICustomFieldCommand _customFieldCommand;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        public CreateCustomFieldCommandhandler(ICustomFieldCommand customFieldCommand, IMapper imapper, IMediator mediator)
        {
            _customFieldCommand = customFieldCommand;
            _imapper = imapper;
            _mediator = mediator;
        }
        public async Task<int> Handle(CreateCustomFieldCommand request, CancellationToken cancellationToken)
        {
             var customField  = _imapper.Map<CustomField>(request);

                var customFieldresult = await _customFieldCommand.CreateAsync(customField);
                
                
                if (customFieldresult > 0)
                {
                //     var domainEvent = new AuditLogsDomainEvent(
                //      actionDetail: "Create",
                //      actionCode: "Create custom field",
                //      actionName: "Create",
                //      details: $"Custom field ",
                //      module:"Custom Field"
                //  );
                //  await _mediator.Publish(domainEvent, cancellationToken);
                 
                    return customFieldresult;
                }
               throw new ValidationException("Custom field not created");
                 
        }
    }
}