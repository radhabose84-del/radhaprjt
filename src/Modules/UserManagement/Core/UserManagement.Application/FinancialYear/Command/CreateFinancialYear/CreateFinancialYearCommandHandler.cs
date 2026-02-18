#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IFinancialYear;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYear;
using UserManagement.Application.FinancialYear.Command.CreateFinancialYear;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Domain.Events;
using Microsoft.EntityFrameworkCore;
using FluentValidation;



namespace UserManagement.Application.FinancialYear.Command.CreateFinancialYear
{
    public class CreateFinancialYearCommandHandler :IRequestHandler<CreateFinancialYearCommand, FinancialYearDto>
    {


     private readonly IFinancialYearCommandRepository  _financialYearCommandRepository;

     private readonly IFinancialYearQueryRepository _financialYearQueryRepository;
        private readonly IMapper _mapper;
          private readonly IMediator _mediator; 
          private readonly ILogger<CreateFinancialYearCommandHandler> _logger;
           

           public CreateFinancialYearCommandHandler(IFinancialYearCommandRepository financialYearCommandRepository,IMapper mapper, IMediator mediator ,ILogger<CreateFinancialYearCommandHandler> logger,IFinancialYearQueryRepository financialYearQueryRepository)
        {
             _financialYearCommandRepository=financialYearCommandRepository;
            _financialYearQueryRepository=financialYearQueryRepository;
            _mapper=mapper;
            _mediator=mediator;
            _logger=logger;
            

        } 
                public async Task<FinancialYearDto> Handle(CreateFinancialYearCommand request, CancellationToken cancellationToken)
          {
            _logger.LogInformation($"Starting CreateFinancialYearCommandHandler for request: {request}" );
           
          //   var financialYears = await _financialYearQueryRepository.GetAllFinancialYearAsync();

           var (financialYears, totalCount) = await _financialYearQueryRepository.GetAllFinancialYearAsync(1, int.MaxValue, null);

                //  var existingFinancialYear = await _financialYearQueryRepository.GetFinancialYearByDateRangeAsync(request.StartDate, request.EndDate);

             var existingFinancialYear = financialYears.FirstOrDefault(fy => fy.StartDate == request.StartDate && fy.EndDate == request.EndDate);

            if (existingFinancialYear != null)
            {
                _logger.LogWarning($"FinancialYear with start year {request.StartYear} already exists." );
                throw new ValidationException("FinancialYear with start year already exists.");
              
            }
            // Map request to entity
            var financialYearEntity = _mapper.Map<UserManagement.Domain.Entities.FinancialYear>(request);
            _logger.LogInformation($"Mapped Create FinancialYear Command to FinancialYear entity: {financialYearEntity}");

            // Save FinancialYear
            var createdfinancialYear = await _financialYearCommandRepository.CreateAsync(financialYearEntity);

          
             if (createdfinancialYear is null)
            {
                _logger.LogWarning($"Failed to create FinancialYear. FinancialYear entity: {financialYearEntity}");
                throw new Exception("FinancialYear not created");
            
            }
            _logger.LogInformation($"FinancialYear successfully created with ID: {financialYearEntity.Id}");

            // Publish Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: financialYearEntity.Id.ToString(),
                actionName: financialYearEntity.StartYear,
                details: $"FinancialYear '{financialYearEntity.StartYear}' was created. FinancialYearID: {financialYearEntity.Id}",
                module: "FinancialYear"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"AuditLogsDomainEvent published for FinancialYear ID: {financialYearEntity}");

            // Map result to DTO
            var financialYearDto = _mapper.Map<FinancialYearDto>(createdfinancialYear);          

            _logger.LogInformation($"Returning success response for FinancialYear ID: { financialYearEntity.Id}");

            return financialYearDto;
        }                 
    }
}