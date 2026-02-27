using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAdditionalCost;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAdditionalCost.Commands.CreateAssetAdditionalCost
{
    public class CreateAssetAdditionalCostCommandHandler : IRequestHandler<CreateAssetAdditionalCostCommand, int>
    {
        private readonly IAssetAdditionalCostCommandRepository _iAssetAdditionalCostCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public CreateAssetAdditionalCostCommandHandler(IAssetAdditionalCostCommandRepository iAssetAdditionalCostCommandRepository, IMediator imediator, IMapper imapper)
        {
            _iAssetAdditionalCostCommandRepository = iAssetAdditionalCostCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
          
        }
        public async Task<int> Handle(CreateAssetAdditionalCostCommand request, CancellationToken cancellationToken)
        {
             var assetAdditionalCost = _imapper.Map<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>(request);
            
            var result = await _iAssetAdditionalCostCommandRepository.CreateAsync(assetAdditionalCost);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: assetAdditionalCost.AssetId.ToString(),
                actionName: assetAdditionalCost.CostType.ToString(),
                details: $"AssetAdditionalCost details was created",
                module: "AssetAdditionalCost");
            await _imediator.Publish(domainEvent, cancellationToken);
            
            if (result > 0)
                  {
                     
                        return  result;
                 }
                 
                 throw new Exception("AssetAdditionalCost Creation Failed");
          
        }
    }
}