using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetGroup;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetGroup.Queries.GetAssetGroupById
{
    public class GetAssetGroupByIdQueryHandler : IRequestHandler<GetAssetGroupByIdQuery,AssetGroupDto>
    {
        private readonly IAssetGroupQueryRepository _iAssetGroupQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetGroupByIdQueryHandler(IAssetGroupQueryRepository iAssetGroupQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetGroupQueryRepository = iAssetGroupQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<AssetGroupDto> Handle(GetAssetGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetGroupQueryRepository.GetByIdAsync(request.Id);
            // Check if the entity exists
            if (result is null)
            {
                throw new ValidationException($"AssetGroup ID {request.Id} not found.");
                
            }
            // Map a single entity
            var assetGroup = _mapper.Map<AssetGroupDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"AssetGroup details {assetGroup.Id} was fetched.",
                    module:"AssetGroup"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return  assetGroup;

        }
    }
}