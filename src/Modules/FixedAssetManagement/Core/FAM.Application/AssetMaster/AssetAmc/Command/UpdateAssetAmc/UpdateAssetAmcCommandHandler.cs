using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAmc.Command.UpdateAssetAmc
{
    public class UpdateAssetAmcCommandHandler : IRequestHandler<UpdateAssetAmcCommand, int>
    {
        private readonly IAssetAmcCommandRepository _iassetamccommandrepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public UpdateAssetAmcCommandHandler(IAssetAmcCommandRepository iassetamccommandrepository, IMediator imediator, IMapper imapper)
        {
            _iassetamccommandrepository = iassetamccommandrepository;
            _imediator = imediator;
            _imapper = imapper; 
        }

        public async Task<int> Handle(UpdateAssetAmcCommand request, CancellationToken cancellationToken)
        {
                var assetamc = _imapper.Map<FAM.Domain.Entities.AssetMaster.AssetAmc>(request);
                // Calculate EndDate based on StartDate and Period (in months)
                if (request.StartDate.HasValue && request.Period.HasValue)
                {
                assetamc.EndDate = request.StartDate.Value.AddMonths(request.Period.Value);
                }
            
                var result = await _iassetamccommandrepository.UpdateAsync(request.Id, assetamc);
                if (result <= 0) // AssetAmc not found
                {
                    throw new ValidationException("AssetAmc not found.");
                    
                }
                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Update",
                    actionCode: assetamc.Id.ToString(),
                    actionName: assetamc.AssetId.ToString(),
                    details: $"AssetAmc details was updated",
                    module: "AssetAmc");
                await _imediator.Publish(domainEvent, cancellationToken);
                return result; 
        }
    }
}