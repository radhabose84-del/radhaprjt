using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IWdvDepreciation;
using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.WDVDepreciation.Queries.CalculateDepreciation
{
    public class GetDepreciationQueryHandler : IRequestHandler<GetDepreciationQuery, List<CalculationDepreciationDto>>
    {        
        private readonly IWdvDepreciationQueryRepository _WdvQueryRepository;
        private readonly IMapper _mapper;

        private readonly IMediator _mediator; 

        public GetDepreciationQueryHandler(IWdvDepreciationQueryRepository WdvQueryRepository,IMapper mapper, IMediator mediator)
        {     
            _WdvQueryRepository = WdvQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<List<CalculationDepreciationDto>> Handle(GetDepreciationQuery request, CancellationToken cancellationToken)
        {
            var depreciationGroups = await _WdvQueryRepository.ExistDataAsync( request.FinYearId);
            if (depreciationGroups==false)
            {
                throw new ValidationException("No details found for this period");
              
            }           
            var WDVCalculation = await _WdvQueryRepository.GetWDVDepreciationAsync(request.FinYearId);
            var WDVCalculationList = _mapper.Map<List<CalculationDepreciationDto>>(WDVCalculation);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Get WDVCalculation",
                actionCode: "",        
                actionName: "",
                details: $"Get WDV Depreciation Calculation.",
                module:"Get WDV Calculation"
            );
            
            await _mediator.Publish(domainEvent, cancellationToken);
            return  WDVCalculationList;            
        }
    }
  
}