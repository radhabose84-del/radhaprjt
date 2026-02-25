using AutoMapper;
using UserManagement.Application.Common.Interfaces.IFinancialYear;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Domain.Events;
using FluentValidation;

namespace UserManagement.Application.FinancialYear.Command.UpdateFinancialYear
{
    public class UpdateFinancialYearCommandHandler  :  IRequestHandler<UpdateFinancialYearCommand, int>
    {
       
        private readonly IFinancialYearCommandRepository _financialYearCommandRepository;
        private readonly IMapper _Imapper;
        private readonly ILogger<UpdateFinancialYearCommandHandler> _logger;
        private readonly IMediator _mediator; 
        
    

    public UpdateFinancialYearCommandHandler(IFinancialYearCommandRepository financialYearCommandRepository,IMapper Imapper, ILogger<UpdateFinancialYearCommandHandler> logger,IMediator mediator)
        {
            _financialYearCommandRepository = financialYearCommandRepository;
            _Imapper = Imapper;
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator;
             
        }

         public async Task<int> Handle(UpdateFinancialYearCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting UpdateFinancialYearCommandHandler for request: {request}" );              
           
            var financialYear = _Imapper.Map<UserManagement.Domain.Entities.FinancialYear>(request);

            var result = await _financialYearCommandRepository.UpdateAsync( request.Id,financialYear);

            if (result <= 0) // Entity not found
            {
                _logger.LogInformation($"FinancialYear {request.Id} not found." );
                throw new ValidationException("FinancialYear not found.");
                
            }

            //Domain Event
             var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: financialYear.Id.ToString(),
            actionName: financialYear.StartDate.ToString(),                            
            details:$"Entity '{financialYear.StartDate}' was Updated. EntityCode: {request.Id}",
            module:"Entity"
            );
            await _mediator.Publish(domainEvent);
            _logger.LogInformation($"Successfully completed UpdateFinancialYearCommandHandler for request: {request.Id}" );
            return result;

        }



        }

}