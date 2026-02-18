using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmc;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmcById
{
    public class GetAssetAmcByIdQueryHandler : IRequestHandler<GetAssetAmcByIdQuery, AssetAmcDto>
    {
        private readonly IAssetAmcQueryRepository _iAssetAmcQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetAssetAmcByIdQueryHandler(IAssetAmcQueryRepository iAssetAmcQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetAmcQueryRepository = iAssetAmcQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<AssetAmcDto> Handle(GetAssetAmcByIdQuery request, CancellationToken cancellationToken)
        {
           var result = await _iAssetAmcQueryRepository.GetByIdAsync(request.Id);
    
            if (result is null)
            {
                throw new ValidationException($"Asset ID {request.Id} not found.");
                
            }
            // Map a single entity
            var assetamc = _mapper.Map<AssetAmcDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "AssetAmc",        
                    actionName: "GetById",
                    details: $"AssetAmc details {assetamc.Id} was fetched.",
                    module:"AssetAmc"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
                return assetamc;
        }
    }
}