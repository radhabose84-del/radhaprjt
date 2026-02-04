using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces.ICurrency;
using UserManagement.Application.Currency.Queries.GetCurrency;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.Currency.Queries.GetCurrencyAutoComplete
{
    public class GetCurrencyAutocompleteQueryHandler : IRequestHandler<GetCurrencyAutocompleteQuery, List<CurrencyAutoCompleteDto>>
    {
        
        private readonly ICurrencyQueryRepository _currencyQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        private readonly ILogger<GetCurrencyAutocompleteQueryHandler> _logger;

        public GetCurrencyAutocompleteQueryHandler(ICurrencyQueryRepository currencyQueryRepository, IMapper mapper, IMediator mediator, ILogger<GetCurrencyAutocompleteQueryHandler> logger)
        {
            _currencyQueryRepository = currencyQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<CurrencyAutoCompleteDto>> Handle(GetCurrencyAutocompleteQuery request, CancellationToken cancellationToken)
        {

            _logger.LogInformation($"Fetching Currency Request started: {request.SearchPattern}");
            var newcurrency = await _currencyQueryRepository.GetByCurrencyNameAsync(request.SearchPattern);
            if (newcurrency is null || !newcurrency.Any() || newcurrency.Count == 0)
            {
                _logger.LogWarning($"No Currency Record {newcurrency.Count} not found in DB.");
                throw new ValidationException("No currency found");
             
            }
            var currencylist = _mapper.Map<List<CurrencyAutoCompleteDto>>(newcurrency);
            _logger.LogInformation($"Fetching Currency Request Completed: {currencylist.Count}");
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetCurrencyAutocompleteQuery",
                actionCode: "Get Currency Autocomplete",                
                actionName: currencylist.Count.ToString(),
                details: $"Currency details was fetched.",
                module:"Currency");
            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"Currency {currencylist.Count} Listed successfully.");
            return currencylist;
        }

    }
}