using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommandHandler : IRequestHandler<DeleteMiscTypeMasterCommand, bool>
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

        public async Task<bool> Handle(DeleteMiscTypeMasterCommand request, CancellationToken cancellationToken)
        {
             var misctypemaster  = _imapper.Map<FAM.Domain.Entities.MiscTypeMaster>(request);
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
                    return misctyperesult;
                }
            throw new Exception("MiscTypeMaster not deleted.");
                
            
        }
    }
}