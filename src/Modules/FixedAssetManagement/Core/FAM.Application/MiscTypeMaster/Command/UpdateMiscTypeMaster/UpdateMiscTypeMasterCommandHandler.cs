using AutoMapper;
using FAM.Application.Common.Interfaces.IMiscTypeMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster
{
    public class UpdateMiscTypeMasterCommandHandler : IRequestHandler<UpdateMiscTypeMasterCommand, bool>
    {
        private readonly IMiscTypeMasterCommandRepository _miscTypeMasterCommandRepository;
        private readonly IMiscTypeMasterQueryRepository _miscTypeMasterQueryRepository;

         private readonly IMapper _imapper;
        private readonly IMediator _mediator;

     public UpdateMiscTypeMasterCommandHandler(IMiscTypeMasterCommandRepository miscTypeMasterCommandRepository ,IMiscTypeMasterQueryRepository miscTypeMasterQueryRepository ,IMapper imapper, IMediator mediator)
        {
            _miscTypeMasterCommandRepository =miscTypeMasterCommandRepository;
            _imapper =imapper;
            _mediator = mediator;
            _miscTypeMasterQueryRepository = miscTypeMasterQueryRepository;
        }

          public async Task<bool> Handle(UpdateMiscTypeMasterCommand request, CancellationToken cancellationToken)
        {
                // Inactivate guard — MUST run BEFORE persisting the update
                if (request.IsActive == 0)
                {
                    var linked = await _miscTypeMasterQueryRepository.IsMiscTypeMasterLinkedAsync(request.Id);
                    if (linked)
                        throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
                }

                var existingMisctype = await _miscTypeMasterQueryRepository.GetByMiscTypeMasterCodeAsync(request.MiscTypeCode ?? string.Empty,request.Id);

                if (existingMisctype != null)
                {
                    throw new ValidationException("MiscTypeMaster already exists");
                    
                }
                 var misctype  = _imapper.Map<FAM.Domain.Entities.MiscTypeMaster>(request);
         
                var misctypemresult = await _miscTypeMasterCommandRepository.UpdateAsync(request.Id, misctype);                

                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Update",
                        actionCode: misctype.MiscTypeCode,
                        actionName: misctype.Description,
                        details: $"MiscTypeMaster '{misctype.Id}' was updated.",
                        module:"MiscTypeMaster"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken); 
              
                if(misctypemresult)
                {
                    return misctypemresult;
                }
            throw new Exception("Misctypemaster result not updated.");
                
           
        }



        
    }
}