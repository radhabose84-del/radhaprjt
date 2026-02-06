using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces.ICurrency;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.Currency.Commands.DeleteCurrency
{
    public class DeleteCurrencyCommandHandler : IRequestHandler<DeleteCurrencyCommand, int>
    {
        private readonly ICurrencyCommandRepository _currencyCommandRepository ;

        private readonly ICurrencyQueryRepository currencyQueryRepository;
        private readonly IMapper _Imapper;
        private readonly IMediator _Imediator;
        private readonly ILogger<DeleteCurrencyCommandHandler> _logger;

        public DeleteCurrencyCommandHandler(ICurrencyCommandRepository currencyCommandRepository,IMapper Imapper,IMediator Imediator,ILogger<DeleteCurrencyCommandHandler> logger,ICurrencyQueryRepository currencyQueryRepository)
        {
            _currencyCommandRepository = currencyCommandRepository;
            _Imapper = Imapper;
            _Imediator = Imediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.currencyQueryRepository = currencyQueryRepository;
        }

    public async Task<int> Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Soft Deleting Currency with ID: {request.Id}");

        var currency = await currencyQueryRepository.GetByIdAsync(request.Id);
        if (currency is null)
        {
            _logger.LogWarning($"Soft Deleting Currency Failed: Currency with ID {request.Id} not found.");
            throw new ValidationException("Currency not found / Currency is deleted.");
        
        }
        var currencydelete = _Imapper.Map<UserManagement.Domain.Entities.Currency>(request);

        var result = await _currencyCommandRepository.DeletecurrencyAsync(request.Id, currencydelete);
        if (result==- 1)
        {
       _logger.LogWarning($"Soft Deleting Currency Failed with ID: {request.Id}");
       throw new ValidationException("Currency not found.");
    
        }
            // Publish domain event for audit logs
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: request.Id.ToString(),
                actionName: "DeleteCurrencyCommand",
                details: $"Currency '{request.Id}' was Deleted.",
                module: "Currency"
            );
            await _Imediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation($"Soft Deleting Currency Successfully Completed with ID: {request.Id}");
            return request.Id;
        
    }
    }
}