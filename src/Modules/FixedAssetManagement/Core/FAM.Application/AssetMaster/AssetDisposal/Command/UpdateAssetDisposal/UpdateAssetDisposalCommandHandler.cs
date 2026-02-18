using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetDisposal;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetDisposal.Command.UpdateAssetDisposal
{
    public class UpdateAssetDisposalCommandHandler : IRequestHandler<UpdateAssetDisposalCommand, int>
    {
         private readonly IAssetDisposalCommandRepository _iassetdisposalcommandrepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public UpdateAssetDisposalCommandHandler(IAssetDisposalCommandRepository iassetdisposalcommandrepository, IMediator imediator, IMapper imapper)
        {
            _iassetdisposalcommandrepository = iassetdisposalcommandrepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<int> Handle(UpdateAssetDisposalCommand request, CancellationToken cancellationToken)
        {
            var assetDisposal = _imapper.Map<FAM.Domain.Entities.AssetMaster.AssetDisposal>(request);
            var result = await _iassetdisposalcommandrepository.UpdateAsync(request.Id, assetDisposal);
            if (result <= 0) // AssetGroup not found
            {
                throw new ValidationException("AssetDisposal Id not found.");
                
            }
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: assetDisposal.Id.ToString(),
                actionName: assetDisposal.AssetId.ToString(),
                details: $"AssetDisposal details was updated",
                module: "AssetDisposal");
            await _imediator.Publish(domainEvent, cancellationToken);
            return  result;  
        }
    }
}