using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces.IFinancialYear;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.FinancialYear.Queries.GetFinancialYearAutoComplete
{
    public class GetFinancialYearAutoCompleteQueryHandler  : IRequestHandler<GetFinancialYearAutoCompleteQuery, List<GetFinancialYearAutoCompleteDto>>
    {
        
        private readonly IFinancialYearCommandRepository _financialYearCommandRepository ;
        private readonly IFinancialYearQueryRepository _financialYearQueryRepository ;
        
        
     private readonly IMapper _mapper;
       private readonly ILogger<GetFinancialYearAutoCompleteQueryHandler> _logger;
     private readonly IMediator _mediator;

      public GetFinancialYearAutoCompleteQueryHandler(IFinancialYearCommandRepository financialYearCommandRepository,IFinancialYearQueryRepository financialYearQueryRepository, IMapper mapper,IMediator mediator, ILogger<GetFinancialYearAutoCompleteQueryHandler> logger  )
      {
            _financialYearCommandRepository = financialYearCommandRepository;
            _financialYearQueryRepository = financialYearQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;

      }

        public async Task<List<GetFinancialYearAutoCompleteDto>> Handle(GetFinancialYearAutoCompleteQuery request, CancellationToken cancellationToken)
        {


              _logger.LogInformation($"Handling GetFinancialYearAutoCompleteQuery with search pattern: {request.SearchTerm}" );
           var financialyear =await _financialYearQueryRepository.GetAllFinancialAutoCompleteSearchAsync(request.SearchTerm);

          

            if (financialyear is null || !financialyear.Any())
            {
                _logger.LogWarning($"No financial years found for search pattern: {request.SearchTerm}" );
                throw new ValidationException("No matching financial years found");
               
            }

            _logger.LogInformation($"Financial years found for search pattern: {request.SearchTerm}. Mapping results to DTO.");

            // Map the result to DTO
            var financialYearDto = _mapper.Map<List<GetFinancialYearAutoCompleteDto>>(financialyear);

            // Publish domain event for audit logs
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutoComplete",
                actionCode: "",
                actionName: request.SearchTerm,
                details: $"FinancialYear '{request.SearchTerm}' was searched",
                module: "FinancialYear"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation($"Domain event published for search pattern: {request.SearchTerm}" );

            return financialYearDto;



           
        }
    }
}