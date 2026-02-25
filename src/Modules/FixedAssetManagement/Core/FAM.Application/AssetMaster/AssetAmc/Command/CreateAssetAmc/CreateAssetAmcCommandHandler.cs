using AutoMapper;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmc;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAmc.Command.CreateAssetAmc
{
    public class CreateAssetAmcCommandHandler : IRequestHandler<CreateAssetAmcCommand, int>
    {
        private readonly IAssetAmcCommandRepository _iassetamccommandrepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public CreateAssetAmcCommandHandler(IAssetAmcCommandRepository iassetamccommandrepository, IMediator imediator, IMapper imapper)
        {
            _iassetamccommandrepository = iassetamccommandrepository;
            _imediator = imediator;
            _imapper = imapper;
          
        }

        public async Task<int> Handle(CreateAssetAmcCommand request, CancellationToken cancellationToken)
        {
            var assetAmc = _imapper.Map<FAM.Domain.Entities.AssetMaster.AssetAmc>(request);


            // Ensure RenewedDate is properly handled
            assetAmc.RenewedDate = request.RenewedDate ?? null;


              // Calculate EndDate based on StartDate and Period (in months)
            if (request.StartDate.HasValue && request.Period.HasValue)
            {
                assetAmc.EndDate = request.StartDate.Value.AddMonths(request.Period.Value);
            }
            
            var result = await _iassetamccommandrepository.CreateAsync(assetAmc);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: assetAmc.AssetId.ToString(),
                actionName: assetAmc.Id.ToString(),
                details: $"AssetAmc details was created",
                module: "AssetAmc");
            await _imediator.Publish(domainEvent, cancellationToken);
             _imapper.Map<AssetAmcDto>(assetAmc);
            if (result > 0)
                  {
                     
                        return  result;
                 }
            throw new Exception("AssetAmc Creation Failed");
        }
    }
}