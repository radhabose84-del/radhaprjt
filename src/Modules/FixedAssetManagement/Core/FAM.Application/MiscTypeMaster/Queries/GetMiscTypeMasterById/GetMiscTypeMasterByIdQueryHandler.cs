using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQueryHandler : IRequestHandler<GetMiscTypeMasterByIdQuery, GetMiscTypeMasterDto>
    {
       private readonly IMiscTypeMasterQueryRepository  _miscTypeMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

         public GetMiscTypeMasterByIdQueryHandler(IMiscTypeMasterQueryRepository miscTypeMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _miscTypeMasterQueryRepository = miscTypeMasterQueryRepository;
            _mapper =mapper;
            _mediator = mediator;
        } 

        public async  Task<GetMiscTypeMasterDto> Handle(GetMiscTypeMasterByIdQuery request, CancellationToken cancellationToken)
        {
                  
            var result = await _miscTypeMasterQueryRepository.GetByIdAsync(request.Id);
            if (result is null )
            {
                throw new ValidationException($"MiscTypeMaster with Id {request.Id} not found.");
              
            }
           
            var misctypemaster = _mapper.Map<GetMiscTypeMasterDto>(result);

            //Domain Event
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "GetById",
                        actionCode: "",        
                        actionName: "",
                        details: $"MiscTypeMaster details {misctypemaster.Id} was fetched.",
                        module:"MiscTypeMaster"
                    );
                    await _mediator.Publish(domainEvent, cancellationToken);
            return misctypemaster;
        }
    }
}