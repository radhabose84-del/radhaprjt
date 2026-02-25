#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IFinancialYear;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.FinancialYear.Command.DeleteFinancialYear
{
    public class DeleteFinancialYearCommandHanlder  :IRequestHandler<DeleteFinancialYearCommand ,int>
    { 
        
         private readonly IFinancialYearCommandRepository  _financialYearCommandRepository;

       private readonly IFinancialYearQueryRepository _financialYearQueryRepository; 
       private readonly IMapper _Imapper;          
      
   
        private readonly IMediator _mediator; 
        private readonly ILogger<DeleteFinancialYearCommandHanlder> _logger;
      

      public DeleteFinancialYearCommandHanlder (IFinancialYearCommandRepository financialYearCommandRepository, IFinancialYearQueryRepository financialYearQueryRepository,IMediator mediator, IMapper mapper,ILogger<DeleteFinancialYearCommandHanlder> logger)
      {

         _financialYearCommandRepository = financialYearCommandRepository;
         _Imapper = mapper;                       
          _financialYearQueryRepository = financialYearQueryRepository;
          _mediator = mediator;
          _logger = logger;
      }
        public async Task<int>Handle(DeleteFinancialYearCommand request, CancellationToken cancellationToken)
      {  


           _logger.LogInformation($"DeleteFinancial YearCommandHandler started for FinancialYear ID: {request.Id}" );

            // Check if FinancialYear exists
            var FinYear = await _financialYearQueryRepository.GetByIdAsync(request.Id);
            if (FinYear is null)
            {
                _logger.LogWarning($"Financial Year with ID {request.Id} not found." );
                throw new ValidationException("FinancialYear not found");
              
            }

            _logger.LogInformation($"FinancialYear with ID {request.Id} found. Proceeding with deletion.");

            // Map request to entity and delete
            var updatedFinancialYear = _Imapper.Map<UserManagement.Domain.Entities.FinancialYear>(request);
            var result = await _financialYearCommandRepository.DeleteAsync(request.Id, updatedFinancialYear);

            if (result <= 0)
            {
                _logger.LogWarning($"Failed to delete FinancialYear with ID {request.Id}.");
                return  result;
            }

            _logger.LogInformation($"FinancialYear with ID {request.Id} deleted successfully.");

            // Publish domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: updatedFinancialYear.FinYearName,
                actionName: updatedFinancialYear.FinYearName,
                details: $"FinancialYear ID: {request.Id} was changed to status inactive.",
                module: "FinancialYear"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"AuditLogsDomainEvent published for FinancialYear ID {request.Id}.");

            return result;  
          

      }

    }
}