#nullable disable
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Application.Common.Interfaces.IWdvDepreciation;
using FAM.Application.WDVDepreciation.Queries.CalculateDepreciation;
using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.WDVDepreciation.Commands.CreateDepreciation
{
    public class CreateDepreciationCommandHandler : IRequestHandler<CreateDepreciationCommand, CalculationDepreciationDto>
    {        
        private readonly IWdvDepreciationQueryRepository _WdvQueryRepository;
        private readonly IMapper _mapper;

        private readonly IMediator _mediator; 

        public CreateDepreciationCommandHandler(IWdvDepreciationQueryRepository WdvQueryRepository, IMapper mapper, IMediator mediator)
        {            
            _WdvQueryRepository = WdvQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<CalculationDepreciationDto> Handle(CreateDepreciationCommand request, CancellationToken cancellationToken)
        {
            var depreciationLocked = await _WdvQueryRepository.ExistDataLockedAsync(request.FinYearId);
            if (depreciationLocked==true)
            {
                throw new ValidationException("Already depreciation details Locked.");
              
            }
            var depreciationGroups = await _WdvQueryRepository.ExistDataAsync( request.FinYearId);
            if (depreciationGroups==true)
            {
                throw new ValidationException("Already depreciation details exist");
              
            }         

            var assetGroup = _mapper.Map<FAM.Domain.Entities.WDVDepreciationDetail>(request);
            var result = await _WdvQueryRepository.CreateAsync(request.FinYearId);            
        
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "WDVCalculation",
                actionCode: "",
                actionName: "",
                details: $"WDV Depreciation Calculation.",
                module: "WDV Calculation"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            if (result != null && result.Any())
            {
                
                return  null;
            }
        throw new Exception("WDV Calculation not created.");
         
        }
    }
  
}