#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using MediatR;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;


namespace FAM.Application.AssetMaster.AssetInsurance.Commands.DeleteAssetInsurance
{
    public class DeleteAssetInsuranceCommandHandler    : IRequestHandler<DeleteAssetInsuranceCommand, bool>
    {  
        private readonly IAssetInsuranceCommandRepository _assetInsuranceCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        public DeleteAssetInsuranceCommandHandler(IAssetInsuranceCommandRepository assetInsuranceCommandRepository , IMapper mapper,  IMediator mediator
         )
        {
            _assetInsuranceCommandRepository = assetInsuranceCommandRepository;
             _mapper = mapper;        
            _mediator = mediator;
            
        }
        public async Task<bool> Handle(DeleteAssetInsuranceCommand request, CancellationToken cancellationToken)
        {
              // Map the request to the entity
            var assetInsuranceDelete = _mapper.Map<FAM.Domain.Entities.AssetMaster.AssetInsurance>(request);

            // Perform the delete operation
            var isDeleted = await _assetInsuranceCommandRepository.DeleteAsync(request.Id, assetInsuranceDelete);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: assetInsuranceDelete.Id.ToString(),
                actionName: assetInsuranceDelete.IsDeleted.ToString(),
                details: $"AssetInsurance with ID {assetInsuranceDelete.Id} was deleted.",
                module: "AssetInsurance"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // Return the response based on the result
            if (isDeleted)
            {
                return isDeleted;
            }
            throw new Exception("AssetInsurance not deleted.");
          
        }
       
    }
}