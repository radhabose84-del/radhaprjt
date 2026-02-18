using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommandHandler : IRequestHandler<DeleteMiscTypeMasterCommand, ApiResponseDTO<GetMiscTypeMasterDto>>
    {

        private readonly IMiscTypeMasterCommandRepository _miscTypeMasterCommandRepository;
      
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;



        public DeleteMiscTypeMasterCommandHandler(IMiscTypeMasterCommandRepository  miscTypeMasterCommandRepository, IMapper imapper , IMediator mediator)
        {
            _miscTypeMasterCommandRepository = miscTypeMasterCommandRepository;
            _imapper = imapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<GetMiscTypeMasterDto>> Handle(DeleteMiscTypeMasterCommand request, CancellationToken cancellationToken)
        {
             var misctypemaster  = _imapper.Map<UserManagement.Domain.Entities.MiscTypeMaster>(request);
            var misctyperesult = await _miscTypeMasterCommandRepository.DeleteAsync(request.Id, misctypemaster);


                  //Domain Event  
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Delete",
                        actionCode: misctypemaster.Id.ToString(),
                        actionName: misctypemaster.IsDeleted.ToString(),
                        details: $"MiscTypeMaster  {misctypemaster.Id} was deleted.",
                        module:"MiscTypeMaster"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken);  

                 if(misctyperesult)
                {
                    return new ApiResponseDTO<GetMiscTypeMasterDto>{IsSuccess = true, Message = "MiscTypeMaster deleted successfully."};
                }

                return new ApiResponseDTO<GetMiscTypeMasterDto>{IsSuccess = false, Message = "MiscTypeMaster not deleted."};
            
        }
    }
}