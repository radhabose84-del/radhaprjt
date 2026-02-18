#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IMiscTypeMaster;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster
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

                var existingMisctype = await _miscTypeMasterQueryRepository.GetByMiscTypeMasterCodeAsync(request.MiscTypeCode,request.Id);

                if (existingMisctype != null)
                {
                    return new ApiResponseDTO<bool>{IsSuccess = false, Message = "MiscTypeMaster already exists"};
                }
                 var misctype  = _imapper.Map<UserManagement.Domain.Entities.MiscTypeMaster>(request);
         
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