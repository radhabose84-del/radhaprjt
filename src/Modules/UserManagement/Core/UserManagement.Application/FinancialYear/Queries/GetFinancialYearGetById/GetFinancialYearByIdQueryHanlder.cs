using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IFinancialYear;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYear;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.FinancialYear.Queries.GetFinancialYearGetById
{
    public class GetFinancialYearByIdQueryHanlder  :IRequestHandler<GetFinancialYearByIdQuery,List<GetFinancialYearDto>>
    {
             
        private readonly IFinancialYearQueryRepository _financialyearRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<GetFinancialYearByIdQueryHanlder> _logger;


    public GetFinancialYearByIdQueryHanlder(IFinancialYearQueryRepository financialyearRepository,IMapper mapper , IMediator mediator, ILogger<GetFinancialYearByIdQueryHanlder> logger)    
    {
        _financialyearRepository = financialyearRepository;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
     }
        public async Task<List<GetFinancialYearDto>> Handle(GetFinancialYearByIdQuery request, CancellationToken cancellationToken)
        {          
          _logger.LogInformation($"Fetching FinancialYear Request started: {request}");

                    // Fetch FinancialYear by ID
                    var financialyear = await _financialyearRepository.GetByIdAsync(request.Id);
                    
                    if (financialyear is null)
                    {
                        _logger.LogWarning($"FinancialYear with ID {request.Id} not found." );
                        throw new ValidationException("FinancialYear not found.");

                    }            

              var financialyearDto = _mapper.Map<GetFinancialYearDto>(financialyear);
                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "Getfinancialyear",        
                    actionName: "Getfinancialyear",                
                    details: $"FinancialYear '{financialyearDto.StartYear}' was created. FinancialYearCode: {financialyearDto.Id}",
                    module:"FinancialYear"
                );

                await _mediator.Publish(domainEvent, cancellationToken);
         
           return new List<GetFinancialYearDto> { financialyearDto };
        }
    }
}