using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Application.Common.Interfaces.IWdvDepreciation;
using FAM.Application.WDVDepreciation.Queries.CalculateDepreciation;
using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.WDVDepreciation.Commands.LockDepreciation
{
    public class LockDepreciationCommandHandler : IRequestHandler<LockDepreciationCommand, CalculationDepreciationDto>
    {
        private readonly IWdvDepreciationCommandRepository _WdvCommandRepository;
        private readonly IWdvDepreciationQueryRepository _WdvQueryRepository;        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public LockDepreciationCommandHandler(IWdvDepreciationCommandRepository WdvCommandRepository ,IWdvDepreciationQueryRepository WdvQueryRepository , IMapper mapper, IMediator mediator)
        {
            _WdvCommandRepository = WdvCommandRepository;
            _WdvQueryRepository = WdvQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<CalculationDepreciationDto> Handle(LockDepreciationCommand request, CancellationToken cancellationToken)
        {
            var depreciationLocked = await _WdvQueryRepository.ExistDataLockedAsync( request.FinYearId);
            if (depreciationLocked==true)
            {
                throw new ValidationException("Already depreciation details Locked.");
             
            }
            var depreciationGroups = await _WdvQueryRepository.ExistDataAsync( request.FinYearId);
            if (depreciationGroups==false)
            throw new ValidationException("No details found for this period");
          

            var depreciationUpdate = _mapper.Map<CalculationDepreciationDto>(request);      
            var updateResult = await _WdvCommandRepository.LockWDVDepreciationAsync(request.FinYearId);
            if (updateResult > 0)
            {
                var depreciationGroupDto = _mapper.Map<CalculationDepreciationDto>(depreciationUpdate);  
                //Domain Event  
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Update",
                    actionCode: depreciationUpdate.FinYearId.ToString() ??string.Empty,
                    actionName: "Update",
                    details: $"WDV Depreciation Locked",
                    module:"DepreciationDetail"
                );               
                await _mediator.Publish(domainEvent, cancellationToken);                 
                return  depreciationGroupDto;
            }
            throw new Exception("Depreciation Locked failed.");
                            
        
        }
    }
  
}