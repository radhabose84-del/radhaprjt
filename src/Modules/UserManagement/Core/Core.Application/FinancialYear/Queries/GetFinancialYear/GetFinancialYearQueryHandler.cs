using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IFinancialYear;
using Core.Application.FinancialYear.Queries.GetFinancialYear;
using Core.Application.GetFinancialYearYear.Queries.GetFinancialYear;
using Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.GetFinancialYear.Queries.GetFinancialYear
{
    public class GetFinancialYearQueryHandler  :IRequestHandler<GetFinancialYearQuery,ApiResponseDTO<List<GetFinancialYearDto>>>
    {
         private readonly IFinancialYearQueryRepository _financialyearRepository;
        private readonly IMapper _mapper; 
         private readonly IMediator _mediator; 

         private readonly ILogger<GetFinancialYearQueryHandler> _logger;
    
       public GetFinancialYearQueryHandler(IFinancialYearQueryRepository financialyearRepository,IMapper mapper , IMediator mediator, ILogger<GetFinancialYearQueryHandler> logger)
        {
            _mapper =mapper;
            _financialyearRepository = financialyearRepository; 
            _mediator = mediator;     
            _logger = logger;

        }
        public async Task<ApiResponseDTO<List<GetFinancialYearDto>>> Handle(GetFinancialYearQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching FinancialYear Request started: {request}", request);

            var (financialYears, totalCount) = await _financialyearRepository.GetAllFinancialYearAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            if (financialYears is null || !financialYears.Any())
            {
                _logger.LogWarning("No FinancialYear records found in the database. Total count: {TotalCount}", totalCount);
                return new ApiResponseDTO<List<GetFinancialYearDto>> { IsSuccess = false, Message = "No Record Found" };
            }

            var financialYearList = _mapper.Map<List<GetFinancialYearDto>>(financialYears);
            
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "FinancialYear details were fetched.",
                module: "FinancialYear"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation("FinancialYear {Count} listed successfully.", financialYearList.Count);
            
            return new ApiResponseDTO<List<GetFinancialYearDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = financialYearList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

      
    }

}