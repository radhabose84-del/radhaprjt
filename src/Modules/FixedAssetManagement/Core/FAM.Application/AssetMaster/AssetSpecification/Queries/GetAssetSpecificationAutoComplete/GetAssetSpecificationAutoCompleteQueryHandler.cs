    using AutoMapper;
    using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
    using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationAutoComplete;
    using Contracts.Common;
    using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
    using FAM.Domain.Events;
using FluentValidation;
using MediatR;

    namespace FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationAutoComplete
    {
        public class GetAssetSpecificationAutoCompleteQueryHandler : IRequestHandler<GetAssetSpecificationAutoCompleteQuery, List<AssetSpecificationJsonDto>>
        {
            private readonly IAssetSpecificationQueryRepository _assetSpecificationRepository;
            private readonly IMapper _mapper;
            private readonly IMediator _mediator; 

            public GetAssetSpecificationAutoCompleteQueryHandler(IAssetSpecificationQueryRepository assetSpecificationRepository,  IMapper mapper, IMediator mediator)
            {
                _assetSpecificationRepository =assetSpecificationRepository;
                _mapper =mapper;
                _mediator = mediator;
            }

            public async Task<List<AssetSpecificationJsonDto>> Handle(GetAssetSpecificationAutoCompleteQuery request, CancellationToken cancellationToken)
            {
                var result = await _assetSpecificationRepository.GetByAssetSpecificationNameAsync(request.SearchPattern ?? string.Empty);
                if (result is null || result.Count is 0)
                {
                    throw new ValidationException("No SpecificationMaster found matching the search pattern.");
                    
                }
                var specificationMasterDto = _mapper.Map<List<AssetSpecificationJsonDto>>(result);
                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAutoComplete",
                    actionCode:"",        
                    actionName: request.SearchPattern ?? string.Empty,                
                    details: $"Asset Specification '{request.SearchPattern}' was searched",
                    module:"Asset Specification"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
                return specificationMasterDto;          
            }      
        }
    }