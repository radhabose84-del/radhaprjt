#nullable disable
using AutoMapper;
using Contracts.Common;
using PartyManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster
{
    public class UpdateMiscTypeMasterCommandHandler : IRequestHandler<UpdateMiscTypeMasterCommand, ApiResponseDTO<bool>>
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

          public async Task<ApiResponseDTO<bool>> Handle(UpdateMiscTypeMasterCommand request, CancellationToken cancellationToken)
        {
                if (request.IsActive == 0)
                {
                    var isLinked = await _miscTypeMasterQueryRepository.IsMiscTypeMasterLinkedAsync(request.Id);
                    if (isLinked)
                        throw new ExceptionRules("This master is linked with other records. You cannot inactivate this record.");
                }

                var existingMisctype = await _miscTypeMasterQueryRepository.GetByMiscTypeMasterCodeAsync(request.MiscTypeCode,request.Id);

                if (existingMisctype != null)
                {
                    return new ApiResponseDTO<bool>{IsSuccess = false, Message = "MiscTypeMaster already exists"};
                }
                 var misctype  = _imapper.Map<PartyManagement.Domain.Entities.MiscTypeMaster>(request);
         
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
                    return new ApiResponseDTO<bool>{IsSuccess = true, Message = "Misctypemresult updated successfully."};
                }

                return new ApiResponseDTO<bool>{IsSuccess = false, Message = "Misctypemresult not updated."};
           
        }
    }
}