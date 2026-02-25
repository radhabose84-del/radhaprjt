#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces.ICurrency;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.Currency.Commands.UpdateCurrency
{
    public class UpdateCurrencyCommandHandler : IRequestHandler<UpdateCurrencyCommand, int>
    {
        private readonly ICurrencyCommandRepository _currencyCommandRepository;

        private readonly ICurrencyQueryRepository _currencyQueryRepository;
        private readonly IMapper _Imapper;
        private readonly ILogger<UpdateCurrencyCommandHandler> _logger;
        private readonly IMediator _mediator; 

        public UpdateCurrencyCommandHandler(ICurrencyCommandRepository currencyCommandRepository,IMapper Imapper,ILogger<UpdateCurrencyCommandHandler> logger,IMediator mediator,ICurrencyQueryRepository currencyQueryRepository)
        {
            _currencyCommandRepository = currencyCommandRepository;
            _Imapper = Imapper;
            _logger = logger;
            _mediator = mediator;
            _currencyQueryRepository = currencyQueryRepository;
        }
 public async Task<int> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
{
    _logger.LogInformation($"Starting Currency Update process for CurrencyId: {request.Id}");

    // 🔹 First, check if the ID exists in the database
    var existingCurrency = await _currencyQueryRepository.GetByIdAsync(request.Id);
    if (existingCurrency is null )
    {
        _logger.LogWarning($"Currency ID {request.Id} not found.");
        throw new ValidationException("Currency Id not found / Currency is deleted .");
    
    }

    // 🔹 Check if currency name already exists for another ID
    var exists = await _currencyCommandRepository.ExistsByNameupdateAsync(request.Name, request.Id);
    if (exists)
    {
        _logger.LogWarning($"Currency Name {request.Name} already exists.");
        throw new ValidationException("Currency Name already exists.");
        
    }

    // 🔹 Map the request to the Currency entity
    var currency = _Imapper.Map<UserManagement.Domain.Entities.Currency>(request);

    // 🔹 Call repository to update the entity
    var result = await _currencyCommandRepository.UpdateAsync(request.Id, currency);

    if (result == -1) // Currency not found
    {
        _logger.LogInformation($"CurrencyId {request.Id} not found.", request.Id);
        throw new ValidationException("Currency Id not found.");
      
    }

    // 🔹 Publish Domain Event for auditing
    var domainEvent = new AuditLogsDomainEvent(
        actionDetail: "Update",
        actionCode: currency.Code,
        actionName: currency.Name,
        details: $"Currency '{currency.Code}' was Updated. CurrencyCode: {request.Id}",
        module: "Currency"
    );    

    await _mediator.Publish(domainEvent, cancellationToken);

    _logger.LogInformation($"CurrencyName {currency.Name} Updated successfully.", currency.Name);

    return result;
}
    }

}