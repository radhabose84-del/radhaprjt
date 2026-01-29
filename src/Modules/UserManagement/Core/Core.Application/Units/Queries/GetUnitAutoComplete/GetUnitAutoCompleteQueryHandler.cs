using MediatR;
using Core.Application.Units.Queries.GetUnits;
using System.Data;
using Core.Application.Common.Interfaces.IUnit;
using AutoMapper;
using Core.Application.Common;
using Core.Domain.Events;

using Core.Application.Common.HttpResponse;
using Microsoft.Extensions.Logging;
using Core.Application.Common.Interfaces;
using FluentValidation;

namespace Core.Application.Units.Queries.GetUnitAutoComplete
{
    public class GetUnitAutoCompleteQueryHandler : IRequestHandler<GetUnitAutoCompleteQuery, List<UnitAutoCompleteDTO>>
    {
         private readonly IUnitQueryRepository _unitRepository;        
        private readonly IMapper _mapper;

        private readonly IMediator _mediator; 

        private readonly ILogger<GetUnitAutoCompleteQueryHandler> _logger;

        private readonly IIPAddressService _ipAddressService;
        public GetUnitAutoCompleteQueryHandler(IUnitQueryRepository unitRepository, IMapper mapper, IMediator mediator,ILogger<GetUnitAutoCompleteQueryHandler> logger,IIPAddressService ipAddressService)
        {
             _unitRepository = unitRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ipAddressService = ipAddressService;
        }

        public async Task<List<UnitAutoCompleteDTO>> Handle(GetUnitAutoCompleteQuery request, CancellationToken cancellationToken)
        {     
            var groupcode = _ipAddressService.GetGroupcode();

            if(groupcode == "SUPER_ADMIN" || groupcode == "ADMIN")
                {
                    var Adminresult = await _unitRepository.GetUnit_SuperAdmin(request.SearchPattern);
                    var AdminunitDto = _mapper.Map<List<UnitAutoCompleteDTO>>(Adminresult);

                    return AdminunitDto; 
                }
           _logger.LogInformation($"Search pattern started: {request.SearchPattern}");

            var userId = _ipAddressService.GetUserId();
            var result = await _unitRepository.GetUnit(request.SearchPattern,userId,request.CompanyId);
              if (result is null || !result.Any() || result.Count == 0) 
                {
                      _logger.LogWarning($"No Unit Record {request.SearchPattern} not found in DB.");
                      throw new ValidationException("Unit not found.");
                    
                }

            var unitDto = _mapper.Map<List<UnitAutoCompleteDTO>>(result);

            //Domain Event            
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetUnitAutoCompleteQuery",
                actionCode:"",        
                actionName: request.SearchPattern,                
                details: $"Unit '{request.SearchPattern}' was searched",
                module:"Unit"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation($"Unit {result.Count} Listed successfully.");
            return unitDto;                                    
        }
    }
}