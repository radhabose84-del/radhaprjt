using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.SubLocation.Queries.GetSubLocations;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.SubLocation.Queries.GetSubLocationById
{
    public class GetSubLocationByIdQueryHandler : IRequestHandler<GetSubLocationByIdQuery, SubLocationDto>
    {
         private readonly ISubLocationQueryRepository _sublocationQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetSubLocationByIdQueryHandler(ISubLocationQueryRepository sublocationQueryRepository,IMapper mapper,IMediator mediator)
        {
            _sublocationQueryRepository = sublocationQueryRepository;
            _mapper = mapper;
            _mediator = mediator;   
        }
        public async Task<SubLocationDto> Handle(GetSubLocationByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _sublocationQueryRepository.GetByIdAsync(request.Id);
             if (result is null)
            {
                throw new ValidationException("SubLocationId not found");
               
            }  
           var sublocation = _mapper.Map<SubLocationDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"SubLocation details {sublocation.Id} was fetched.",
                    module:"SubLocation"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return sublocation;
        }
    }
}