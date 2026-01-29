using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.ICurrency;
using Core.Application.Currency.Queries.GetCurrency;
using Core.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.Currency.Queries.GetCurrencyById
{
    public class GetCurrencyByIdQueryHandler : IRequestHandler<GetCurrencyByIdQuery, CurrencyDto>
    {
        private readonly ICurrencyQueryRepository _currencyQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<GetCurrencyByIdQueryHandler> _logger;

        public GetCurrencyByIdQueryHandler(ICurrencyQueryRepository currencyQueryRepository, IMapper mapper, IMediator mediator, ILogger<GetCurrencyByIdQueryHandler> logger)
        {
            _currencyQueryRepository = currencyQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CurrencyDto> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Fetching Currency Request started: {request.CurrencyId}");
            var newcurrency = await _currencyQueryRepository.GetByIdAsync(request.CurrencyId);
            if (newcurrency is null)
            {
                _logger.LogWarning($"No Currency Record {request.CurrencyId} not found in DB.");
                throw new ValidationException($"Currency ID {request.CurrencyId} not found.");
           
            }
            var currencylist = _mapper.Map<CurrencyDto>(newcurrency);
            _logger.LogInformation($"Fetching Currency Request Completed: {currencylist.Id}");
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetCurrencyByIdQuery",
                actionCode: "Get Currency",                
                actionName: "",
                details: $"Currency details was fetched.",
                module:"Currency");
            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"Currency {currencylist.Id} Listed successfully.");
            return currencylist;
        }
    }

}