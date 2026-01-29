using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposal;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetDisposal;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposalById
{
    public class GetAssetDisposalByIdQueryHandler : IRequestHandler<GetAssetDisposalByIdQuery,AssetDisposalDto>
    {
        
        private readonly IAssetDisposalQueryRepository _iAssetDisposalQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetDisposalByIdQueryHandler(IAssetDisposalQueryRepository iAssetDisposalQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetDisposalQueryRepository = iAssetDisposalQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<AssetDisposalDto> Handle(GetAssetDisposalByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetDisposalQueryRepository.GetByIdAsync(request.Id);
            // Check if the entity exists
            if (result is null)
            {
                throw new ValidationException( $"Asset ID {request.Id} not found.");
                
            }
            // Map a single entity
            var assetDisposal = _mapper.Map<AssetDisposalDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAssetDisposalByIdQuery",
                    actionCode: "GetById",        
                    actionName: request.Id.ToString(),
                    details: $"AssetAdditionalCost details {assetDisposal.Id} was fetched.",
                    module:"AssetDisposal"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return assetDisposal;
        }
        
    }
}