using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Commands.CreateAssetPurchaseDetails
{
    public class CreateAssetPurchaseDetailCommandHandler  : IRequestHandler<CreateAssetPurchaseDetailCommand, int>
    {
        private readonly IAssetPurchaseCommandRepository _iassetPurchaseCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

          public CreateAssetPurchaseDetailCommandHandler(IAssetPurchaseCommandRepository iassetPurchaseCommandRepository, IMediator imediator, IMapper imapper)
        {
            _iassetPurchaseCommandRepository = iassetPurchaseCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
            
        }

        public async Task<int> Handle(CreateAssetPurchaseDetailCommand request, CancellationToken cancellationToken)
        {
            var assetPurchaseDetails = _imapper.Map<FAM.Domain.Entities.AssetPurchase.AssetPurchaseDetails>(request);
             
             // Ensure CapitalizationDate is properly handled
            assetPurchaseDetails.CapitalizationDate = request.CapitalizationDate ?? null;

            var result = await _iassetPurchaseCommandRepository.CreateAsync(assetPurchaseDetails);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: assetPurchaseDetails.Id.ToString(),
                actionName: assetPurchaseDetails.ItemName,
                details: $"AssetPurchase details was created",
                module: "AssetPurchaseDetails");
            await _imediator.Publish(domainEvent, cancellationToken);
            
             
            if (result > 0)
                  {
                    
                        return result;
                 }

                 throw new Exception("AssetPurchase Creation Failed");
        }
    }
}