using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.ICurrency;
using Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.Currency.Queries.GetCurrency
{
    public class GetCurrencyQueryHandler  : IRequestHandler<GetCurrencyQuery, ApiResponseDTO<List<CurrencyDto>>>
    {
        private readonly ICurrencyQueryRepository _currencyQueryRepository;        
        private readonly IMapper _mapper;

        private readonly IMediator _mediator;

        private readonly ILogger<GetCurrencyQueryHandler> _logger;

        public GetCurrencyQueryHandler(ICurrencyQueryRepository currencyQueryRepository, IMapper mapper, IMediator mediator, ILogger<GetCurrencyQueryHandler> logger)
        {
            _currencyQueryRepository = currencyQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
         _logger = logger?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponseDTO<List<CurrencyDto>>> Handle(GetCurrencyQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Fetching Currency Request started: {request}");
            var (newcurrency,totalCount) = await _currencyQueryRepository.GetAllCurrencyAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var currencylist = _mapper.Map<List<CurrencyDto>>(newcurrency);
            _logger.LogInformation($"Fetching Currency Request Completed: {currencylist.Count}");
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetCurrencyQuery",
                actionCode: "Get Currency",        
                actionName: currencylist.Count.ToString(),
                details: $"Currency details was fetched.",
                module:"Currency");
            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"Currency {currencylist.Count} Listed successfully.");
            return new ApiResponseDTO<List<CurrencyDto>>
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = currencylist,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}