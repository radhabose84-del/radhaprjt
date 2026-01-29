using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Application.Common.Interfaces.IWdvDepreciation;
using FAM.Application.WDVDepreciation.Queries.CalculateDepreciation;
using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.WDVDepreciation.Commands.DeleteDepreciation
{
    public class DeleteDepreciationCommandHandler : IRequestHandler<DeleteDepreciationCommand, CalculationDepreciationDto>
    {
        private readonly IWdvDepreciationCommandRepository _WdvCommandRepository;
        private readonly IWdvDepreciationQueryRepository _WdvQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public DeleteDepreciationCommandHandler(IWdvDepreciationCommandRepository WdvCommandRepository ,IWdvDepreciationQueryRepository WdvQueryRepository, IMapper mapper, IMediator mediator)
        {
            _WdvCommandRepository = WdvCommandRepository;
            _WdvQueryRepository = WdvQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<CalculationDepreciationDto> Handle(DeleteDepreciationCommand request, CancellationToken cancellationToken)
        {
            var depreciationLocked = await _WdvQueryRepository.ExistDataLockedAsync(request.FinYearId);
            if (depreciationLocked==true)
            {
                throw new ValidationException("Already depreciation details Locked.");
               
            }
            var depreciationGroups = await _WdvQueryRepository.ExistDataAsync( request.FinYearId);
            if (depreciationGroups==false)
            {
                throw new ValidationException("No details found for this period");
             
            }

            var depreciationDelete = _mapper.Map<CalculationDepreciationDto>(request);      
            var updateResult = await _WdvCommandRepository.DeleteAsync( request.FinYearId);
            if (updateResult > 0)
            {
                var depreciationGroupDto = _mapper.Map<CalculationDepreciationDto>(depreciationDelete);  
                //Domain Event  
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Delete",
                    actionCode: depreciationDelete.FinYear.ToString() ??string.Empty,
                    actionName: "Delete",
                    details: $"WDV Depreciation Details was Deleted. ",
                    module:"WDV Depreciation Deletion"
                );               
                await _mediator.Publish(domainEvent, cancellationToken);                 
                return  depreciationGroupDto;
            }
            throw new ValidationException("Depreciation deletion failed.");
                     
        }
    }
  
}