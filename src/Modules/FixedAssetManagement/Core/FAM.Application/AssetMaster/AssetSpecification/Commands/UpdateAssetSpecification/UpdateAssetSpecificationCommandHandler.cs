using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetSpecification.Commands.UpdateAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetSpecification.Commands.UpdateAssetSpecification
{
    public class UpdateAssetSpecificationCommandHandler : IRequestHandler<UpdateAssetSpecificationCommand, string>
    {
        private readonly IAssetSpecificationCommandRepository _assetSpecificationRepository;
        private readonly IAssetSpecificationQueryRepository _assetSpecificationQueryRepository;
        private readonly IMediator _mediator;

        public UpdateAssetSpecificationCommandHandler(
            IAssetSpecificationCommandRepository assetSpecificationRepository,
            IAssetSpecificationQueryRepository assetSpecificationQueryRepository,
            IMediator mediator)
        {
            _assetSpecificationRepository = assetSpecificationRepository;
            _assetSpecificationQueryRepository = assetSpecificationQueryRepository;
            _mediator = mediator;
        }

        public async Task<string> Handle(UpdateAssetSpecificationCommand request, CancellationToken cancellationToken)
        {
            // Fetch existing specifications for the asset
            var existingSpecs = await _assetSpecificationQueryRepository.GetByIdAsync(request.AssetId);
            if (existingSpecs == null)
            {
                throw new ValidationException("Asset not found or specifications are not available.");
               
            }

            int updateCount = 0;

            // Iterate through each specification to update
            foreach (var spec in request.Specifications)
            {
                var exists = await _assetSpecificationRepository.ExistsByAssetSpecIdAsync(request.AssetId, spec.SpecificationId);
                if (exists)
                {
                    // Prepare the updated specification
                    var updatedSpec = new AssetSpecifications
                    {
                        AssetId = request.AssetId,
                        SpecificationId = spec.SpecificationId,
                        SpecificationValue = spec.SpecificationValue,
                        // Ensure correct mapping for IsActive field
                        IsActive = spec.IsActive == 1 ? BaseEntity.Status.Active : BaseEntity.Status.Inactive
                    };

                    // Update the specification in the repository
                    await _assetSpecificationRepository.UpdateAsync(request.AssetId, updatedSpec);
                    updateCount++;

                    // Publish domain event for the update
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Update",
                        actionCode: request.AssetId.ToString(),
                        actionName: spec.SpecificationId.ToString(),
                        details: $"Updated specification '{spec.SpecificationId}' to value '{spec.SpecificationValue}'",
                        module: "AssetSpecification"
                    );

                    await _mediator.Publish(domainEvent, cancellationToken);
                }
            }

            // Return a response based on whether any updates were made
            return  updateCount > 0 ? "Specifications updated successfully." : "No specifications updated.";
        }
    }
}
