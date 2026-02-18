#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetSpecification.Commands.CreateAssetSpecification
{
    public class CreateAssetSpecificationCommandHandler : IRequestHandler<CreateAssetSpecificationCommand,string>
    {
        private readonly IMapper _mapper;
        private readonly IAssetSpecificationCommandRepository _assetSpecificationRepository;
        private readonly IMediator _mediator;

        public CreateAssetSpecificationCommandHandler(IMapper mapper, IAssetSpecificationCommandRepository assetSpecificationRepository, IMediator mediator)
        {
            _mapper = mapper;
            _assetSpecificationRepository = assetSpecificationRepository;
            _mediator = mediator;    
        } 

        public async Task<string> Handle(CreateAssetSpecificationCommand request, CancellationToken cancellationToken)
        {
             var createdCount = 0;

            foreach (var spec in request.Specifications )
            {
                var alreadyExists = await _assetSpecificationRepository.ExistsByAssetSpecIdAsync(request.AssetId, spec.SpecificationId);
                if (!alreadyExists)
                {
                    var assetSpecification = new AssetSpecifications
                    {
                        AssetId = request.AssetId,
                        SpecificationId = spec.SpecificationId,
                        SpecificationValue = spec.SpecificationValue                                                                        
                    };

                    await _assetSpecificationRepository.CreateAsync(assetSpecification);
                    createdCount++;

                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Create",
                        actionCode: spec.SpecificationId.ToString() ?? string.Empty,
                        actionName: spec.SpecificationName ?? string.Empty,
                        details: $"Asset Specification '{spec.SpecificationValue}' created ",
                        module: "Asset Specification"
                    );
                    await _mediator.Publish(domainEvent, cancellationToken);
                }
                else{
                  
                    throw new ValidationException("Already Exists");
                      
                }
            }
            return  createdCount > 0 ? "Specifications saved successfully." : "No new specifications were saved.";  
        }
    }
}