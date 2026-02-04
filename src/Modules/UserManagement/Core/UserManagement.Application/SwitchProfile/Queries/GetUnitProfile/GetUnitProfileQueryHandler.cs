using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IProfile;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.SwitchProfile.Queries.GetUnitProfile
{
    public class GetUnitProfileQueryHandler : IRequestHandler<GetUnitProfileQuery, List<GetUnitProfileDTO>>
    {
        private readonly IProfileQuery _iProfileQuery;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator; 
        public GetUnitProfileQueryHandler(IProfileQuery iProfileQuery, IMapper mapper, IIPAddressService ipAddressService,IMediator mediator)
        {
            _iProfileQuery = iProfileQuery;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }
        public async Task<List<GetUnitProfileDTO>> Handle(GetUnitProfileQuery request, CancellationToken cancellationToken)
        {
            var userId = _ipAddressService.GetUserId();
            var result = await _iProfileQuery.GetUnit(userId);

              if (result is null || !result.Any() || result.Count == 0) 
                {
                      throw new ValidationException("Unit not found.");
                   
                }
                var unitDto = _mapper.Map<List<GetUnitProfileDTO>>(result);

                      var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetUnitProfileQuery",
                actionCode:"",        
                actionName: "GetUnitProfileQuery",                
                details: $"Unit was searched",
                module:"Unit"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            
            return unitDto; 
        }
    }
}