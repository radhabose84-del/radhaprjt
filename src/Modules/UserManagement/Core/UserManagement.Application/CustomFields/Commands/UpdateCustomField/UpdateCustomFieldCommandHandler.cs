using AutoMapper;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Domain.Entities;
using MediatR;

namespace UserManagement.Application.CustomFields.Commands.UpdateCustomField
{
    public class UpdateCustomFieldCommandHandler : IRequestHandler<UpdateCustomFieldCommand, bool>
    {
        private readonly ICustomFieldCommand _customFieldCommand;
         private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        public UpdateCustomFieldCommandHandler(ICustomFieldCommand customFieldCommand, IMapper imapper, IMediator mediator)
        {
            _customFieldCommand = customFieldCommand;
            _imapper = imapper;
            _mediator = mediator;
        }
        public async Task<bool> Handle(UpdateCustomFieldCommand request, CancellationToken cancellationToken)
        {
              var customField  = _imapper.Map<CustomField>(request);
         
                var customFieldresult = await _customFieldCommand.UpdateAsync(customField);

                
                    // var domainEvent = new AuditLogsDomainEvent(
                    //     actionDetail: "Update",
                    //     actionCode: "Update custom field",
                    //     actionName: "Update",
                    //     details: $"Custom field '{request.Id}' was updated.",
                    //     module:"Custom Field"
                    // );               
                    // await _mediator.Publish(domainEvent, cancellationToken); 
              
                if(customFieldresult)
                {
                    return customFieldresult;
                }
            throw new Exception("Custom field not updated.");
              
        }
    }
}