using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAdditionalCost;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAdditionalCost.Commands.UpdateAssetAdditionalCost
{
    public class UpdateAssetAdditionalCostCommandHandler : IRequestHandler<UpdateAssetAdditionalCostCommand, int>
    {
        private readonly IAssetAdditionalCostCommandRepository _iAssetAdditionalCostCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public UpdateAssetAdditionalCostCommandHandler(IAssetAdditionalCostCommandRepository iAssetAdditionalCostCommandRepository, IMediator imediator, IMapper imapper)
        {
            _iAssetAdditionalCostCommandRepository = iAssetAdditionalCostCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
          
        }

        public async Task<int> Handle(UpdateAssetAdditionalCostCommand request, CancellationToken cancellationToken)
        {
        var assetAdditionalCost = _imapper.Map<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>(request);
        var result = await _iAssetAdditionalCostCommandRepository.UpdateAsync(request.Id, assetAdditionalCost);
        if (result <= 0) // AssetGroup not found
        {
            throw new ValidationException("AssetMasterId not found.");
            
        }
        //Domain Event
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: assetAdditionalCost.Id.ToString(),
            actionName: assetAdditionalCost.CostType.ToString(),
            details: $"AssetAdditionalCost details was updated",
            module: "AssetAdditionalCost");
        await _imediator.Publish(domainEvent, cancellationToken);
        return result;  
        }
    }
}