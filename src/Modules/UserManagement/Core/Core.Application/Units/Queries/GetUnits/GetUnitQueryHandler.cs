using AutoMapper;
using Core.Application.Common;

using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IUnit;
using Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Core.Application.Units.Queries.GetUnits
{
    public class GetUnitQueryHandler : IRequestHandler<GetUnitQuery,ApiResponseDTO<List<GetUnitsDTO>>>
    {
        private readonly IUnitQueryRepository _unitRepository;        
        private readonly IMapper _mapper;

        private readonly IMediator _mediator;
         private readonly ILogger<GetUnitQueryHandler> _logger;
        public GetUnitQueryHandler( IUnitQueryRepository unitRepository, IMapper mapper, IMediator mediator,ILogger<GetUnitQueryHandler> logger)
        {
            _unitRepository = unitRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponseDTO<List<GetUnitsDTO>>> Handle(GetUnitQuery request, CancellationToken cancellationToken)
        {
        
             _logger.LogInformation($"Fetching Unit Request started: {request}");
            var (units, totalCount) = await _unitRepository.GetAllUnitsAsync(request.PageNumber, request.PageSize, request.SearchTerm);
             if (units is null )
                {
                 _logger.LogWarning($"No Unit Record {units.Count} not found in DB.");
                     return new ApiResponseDTO<List<GetUnitsDTO>>
                     {
                         IsSuccess = false,
                         Message = "No Unit found"
                     };
                }
            var unitList = _mapper.Map<List<GetUnitsDTO>>(units);
            _logger.LogInformation($"Fetching Unit Request Completed: {units.Count}" );
            //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetUnitQuery",
                    actionCode: "Get Units",        
                    actionName: "Get",
                    details: $"Units details was fetched.",
                    module:"Unit"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
                _logger.LogInformation($"Unit {units.Count} Listed successfully.");
                return new ApiResponseDTO<List<GetUnitsDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = unitList,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
           
        
        }
    }
}