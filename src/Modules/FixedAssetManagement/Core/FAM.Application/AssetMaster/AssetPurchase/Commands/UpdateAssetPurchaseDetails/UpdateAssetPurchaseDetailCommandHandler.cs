using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Commands.UpdateAssetPurchaseDetails
{
    public class UpdateAssetPurchaseDetailCommandHandler : IRequestHandler<UpdateAssetPurchaseDetailCommand, int>
    {
        private readonly  IAssetPurchaseCommandRepository _iassetPurchaseCommandRepository;
        private readonly IAssetPurchaseQueryRepository _iAssetPurchaseQueryRepository;
        private readonly IMapper _Imapper;
        private readonly IMediator _mediator;
        public UpdateAssetPurchaseDetailCommandHandler(IAssetPurchaseCommandRepository iassetPurchaseCommandRepository, IMapper imapper, IMediator mediator, IAssetPurchaseQueryRepository iAssetPurchaseQueryRepository)
        {
            _iassetPurchaseCommandRepository = iassetPurchaseCommandRepository;
            _Imapper = imapper;
            _mediator = mediator;
            _iAssetPurchaseQueryRepository = iAssetPurchaseQueryRepository;
        }

        public async Task<int> Handle(UpdateAssetPurchaseDetailCommand request, CancellationToken cancellationToken)
        {
             // 🔹 First, check if the ID exists in the database
        var existingassetpurchaseId = await _iAssetPurchaseQueryRepository.GetByIdAsync(request.Id);
        if (existingassetpurchaseId is null)
        {
         throw new ValidationException("AssetPurchaseDetails Id not found .");
     
        }
         var assetPurchaseDetails = _Imapper.Map<FAM.Domain.Entities.AssetPurchase.AssetPurchaseDetails>(request);
        var result = await _iassetPurchaseCommandRepository.UpdateAsync(request.Id, assetPurchaseDetails);
        if (result <= 0) // AssetGroup not found
        {
          throw new ValidationException("AssetPurchaseDetails not found.");
            
        }
        //Domain Event
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: assetPurchaseDetails.Id.ToString(),
            actionName: assetPurchaseDetails.ItemName,
            details: $"AssetPurchase details was updated",
            module: "AssetPurchaseDetails");
        await _mediator.Publish(domainEvent, cancellationToken);
        return result;   

        }
    }
}