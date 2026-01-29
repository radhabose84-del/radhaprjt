using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchase;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchaseById
{
    public class GetAssetPurchaseByIdQueryHandler : IRequestHandler<GetAssetPurchaseByIdQuery,AssetPurchaseDetailsDto>
    {
         private readonly IAssetPurchaseQueryRepository _iAssetPurchaseQueryRepository;  
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetAssetPurchaseByIdQueryHandler(IAssetPurchaseQueryRepository iAssetPurchaseQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetPurchaseQueryRepository = iAssetPurchaseQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<AssetPurchaseDetailsDto> Handle(GetAssetPurchaseByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetPurchaseQueryRepository.GetByIdAsync(request.Id);
            // Check if the entity exists
            if (result is null)
            {
                throw new ValidationException($"AssetPurchase ID {request.Id} not found.");
                
            }
            // Map a single entity
            var assetGroup = _mapper.Map<AssetPurchaseDetailsDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"Asset Purchase details {request.Id} was fetched.",
                    module:"AssetPurchaseDetails"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return  assetGroup;
        }
    }
}