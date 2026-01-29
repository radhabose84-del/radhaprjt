using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IMiscMaster;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQueryHandler : IRequestHandler<GetMiscMasterByIdQuery, GetMiscMasterDto>
    {

        private readonly IMiscMasterQueryRepository  _miscMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

         public GetMiscMasterByIdQueryHandler(IMiscMasterQueryRepository miscMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mapper =mapper;
            _mediator = mediator;
        } 

          public async  Task<GetMiscMasterDto> Handle(GetMiscMasterByIdQuery request, CancellationToken cancellationToken)
        {
                  
            var result = await _miscMasterQueryRepository.GetByIdAsync(request.Id);
            if (result is null )
            {
                throw new ValidationException($"MiscTypeMaster with Id {request.Id} not found.");
              
            }
           
            var misctypemaster = _mapper.Map<GetMiscMasterDto>(result);

            //Domain Event
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "GetById",
                        actionCode: "",        
                        actionName: "",
                        details: $"MiscTypeMaster details {misctypemaster.Id} was fetched.",
                        module:"MiscTypeMaster"
                    );
                    await _mediator.Publish(domainEvent, cancellationToken);
            return  misctypemaster;
        }

        
    }
}