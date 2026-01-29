using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.ITimeZones;
using Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.TimeZones.Queries.GetTimeZones
{
    public class GetTimeZonesQueryHandler : IRequestHandler<GetTimeZonesQuery, ApiResponseDTO<List<TimeZonesDto>>>
    {
        private readonly ITimeZonesQueryRepository _timeZonesQueryRepository;        
        private readonly IMapper _mapper;

        private readonly IMediator _mediator;

        private readonly ILogger<GetTimeZonesQueryHandler> _logger;

        public GetTimeZonesQueryHandler(ITimeZonesQueryRepository timeZonesQueryRepository, IMapper mapper, IMediator mediator, ILogger<GetTimeZonesQueryHandler> logger)
        {
            _timeZonesQueryRepository = timeZonesQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
          _logger = logger?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponseDTO<List<TimeZonesDto>>> Handle(GetTimeZonesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Fetching TimeZones Request started: {request}");
            var (newTimeZones,totalCount) = await _timeZonesQueryRepository.GetAllTimeZonesAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            if (newTimeZones is null || !newTimeZones.Any() || newTimeZones.Count == 0)
            {
                _logger.LogWarning($"No TimeZones Record {newTimeZones.Count} not found in DB.");
                return new ApiResponseDTO<List<TimeZonesDto>>
                {
                    IsSuccess = false,
                    Message = "No TimeZones found"
                };
            }
            var TimeZoneslist = _mapper.Map<List<TimeZonesDto>>(newTimeZones);
            _logger.LogInformation($"Fetching TimeZones Request Completed: {TimeZoneslist.Count}");
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetTimeZonesQuery",
                actionCode: "Get TimeZones",                
                actionName: TimeZoneslist.Count.ToString(),
                details: $"TimeZones details was fetched.",
                module:"TimeZones");            
            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"TimeZones {TimeZoneslist.Count} Listed successfully.");
            return new ApiResponseDTO<List<TimeZonesDto>>
            {   
                IsSuccess = true,                
                Message = "Success",
                Data = TimeZoneslist,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}