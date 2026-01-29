using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCost;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAdditionalCost;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCostById
{
    public class GetAssetAdditionalCostByIdQueryHandler : IRequestHandler<GetAssetAdditionalCostByIdQuery,AssetAdditionalCostDto>
    {
        private readonly IAssetAdditionalCostQueryRepository _iAssetAdditionalCostQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetAdditionalCostByIdQueryHandler(IAssetAdditionalCostQueryRepository iAssetAdditionalCostQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetAdditionalCostQueryRepository = iAssetAdditionalCostQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<AssetAdditionalCostDto> Handle(GetAssetAdditionalCostByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetAdditionalCostQueryRepository.GetByIdAsync(request.Id);
            // Check if the entity exists
            if (result is null)
            {
                throw new ValidationException($"AssetAdditionalCost ID {request.Id} not found.");
                
            }
            // Map a single entity
            var assetGroup = _mapper.Map<AssetAdditionalCostDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "AssetAdditionalCost",        
                    actionName: "AssetAdditionalCost",
                    details: $"AssetAdditionalCost details {assetGroup.Id} was fetched.",
                    module:"AssetAdditionalCost"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return assetGroup;
        }
    }
}