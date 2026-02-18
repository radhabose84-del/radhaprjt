using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetLocation.Queries.GetAssetLocation;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetLocation.Commands.CreateAssetLocation
{
    public class CreateAssetLocationCommandHandler  : IRequestHandler<CreateAssetLocationCommand, AssetLocationDto>
    {
        private readonly IAssetLocationCommandRepository _assetLocationCommandRepository;
        private readonly IAssetLocationQueryRepository _assetLocationQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateAssetLocationCommandHandler(
            IAssetLocationCommandRepository assetLocationCommandRepository,
            IMapper mapper,
            IMediator mediator,
            IAssetLocationQueryRepository assetLocationQueryRepository)
        {
            _assetLocationCommandRepository = assetLocationCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _assetLocationQueryRepository = assetLocationQueryRepository;
        }

        public async Task<AssetLocationDto> Handle(CreateAssetLocationCommand request, CancellationToken cancellationToken)
        {
            // Check if AssetLocation with the same AssetId already exists
            var existingAssetLocation = await _assetLocationQueryRepository.GetByIdAsync(request.AssetId);
            
            if (existingAssetLocation != null)
            {
                throw new ValidationException("Asset Location already exists");
               
            }

            // Map request to domain entity
            var assetLocation = _mapper.Map<FAM.Domain.Entities.AssetMaster.AssetLocation>(request);

            // Insert into the database
            var result = await _assetLocationCommandRepository.CreateAsync(assetLocation);
            if (result.Id <= 0)
            {
                throw new Exception("Failed to create Asset Location");
               
            }

            // Fetch newly created record
            var createdAssetLocation = await _assetLocationQueryRepository.GetByIdAsync(result.AssetId);
            var mappedResult = _mapper.Map<AssetLocationDto>(createdAssetLocation);

            // Publish domain event for auditing/logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: mappedResult.AssetId.ToString(),
                actionName: mappedResult.LocationId.ToString(),
                details: $"Asset Location '{mappedResult.AssetId}' was created at Location: {mappedResult.LocationId}",
                module: "AssetLocation"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            // Return success response
            return  mappedResult;
        }
    }

}