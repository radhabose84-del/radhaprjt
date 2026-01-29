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

namespace Core.Application.Units.Queries.GetUnitByUserId
{
    public class GetUnitByUserIdQueryHandler : IRequestHandler<GetUnitByUserIdQuery, List<UnitAutoCompleteDTO>>
    {
         private readonly IUnitQueryRepository _unitRepository;        
        private readonly IMapper _mapper;

        private readonly IMediator _mediator; 

        private readonly ILogger<GetUnitByUserIdQueryHandler> _logger;

        private readonly IIPAddressService _ipAddressService;
        public GetUnitByUserIdQueryHandler(IUnitQueryRepository unitRepository, IMapper mapper, IMediator mediator,ILogger<GetUnitByUserIdQueryHandler> logger,IIPAddressService ipAddressService)
        {
             _unitRepository = unitRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ipAddressService = ipAddressService;
        }

        public async Task<List<UnitAutoCompleteDTO>> Handle(GetUnitByUserIdQuery request, CancellationToken cancellationToken)
        { 
            //var userId = _ipAddressService.GetUserId();
            var result = await _unitRepository.GetUnitByUserId(request.UserId,request.CompanyId);
              if (result is null || !result.Any() || result.Count == 0) 
                {
                      _logger.LogWarning($"No Unit Record found in DB.");
                      throw new ValidationException("Unit not found.");
                    
                }

            var unitDto = _mapper.Map<List<UnitAutoCompleteDTO>>(result);

            //Domain Event            
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetUnitByUserIdQuery",
                actionCode:"",        
                actionName: "",                
                details: $"Unit  was searched",
                module:"Unit"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation($"Unit {result.Count} Listed successfully.");
            return unitDto;                                    
        }
    }
}