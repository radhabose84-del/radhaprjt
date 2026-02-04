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

namespace UserManagement.Application.Currency.Commands.CreateCurrency
{
    public class CreateCurrencyCommandHandler  :  IRequestHandler<CreateCurrencyCommand, int>
    {
        private readonly ICurrencyCommandRepository _currencyCommandRepository ;
        private readonly IMapper _Imapper;
        private readonly IMediator _Imediator;
        private readonly ILogger<CreateCurrencyCommandHandler> _logger;

        public CreateCurrencyCommandHandler(ICurrencyCommandRepository currencyCommandRepository,IMapper Imapper,IMediator Imediator,ILogger<CreateCurrencyCommandHandler> logger)
        {
            _currencyCommandRepository = currencyCommandRepository;
            _Imapper = Imapper;
            _Imediator = Imediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting creation process for Currency: {request}");
            // Check if currency code already exists
            var exists = await _currencyCommandRepository.ExistsByCodeAsync(request.Code);
            if (exists)
            {
                 _logger.LogWarning($"Currency Code {request.Code} already exists.");
                 throw new ValidationException("Currency Code already exists.");
               
            }
            // Map the request to the Core domain Currency            
            var currency = _Imapper.Map<UserManagement.Domain.Entities.Currency>(request);

            var result = await _currencyCommandRepository.CreateAsync(currency);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: currency.Code, 
                actionName: currency.Name, 
                details: $"Currency details was created",
                module:"Currency");
                await _Imediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"Currency {currency.Name} Created successfully.");
             

             if (result > 0)
                  {
                     _logger.LogInformation($"Currency {result} created successfully");
                        return result;
                 }
                 throw new Exception("Currency Creation Failed");
           
           
            
        }

    }
}