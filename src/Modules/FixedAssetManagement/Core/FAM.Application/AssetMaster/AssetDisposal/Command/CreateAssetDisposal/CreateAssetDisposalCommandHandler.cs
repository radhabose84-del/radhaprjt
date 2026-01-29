using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposal;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetDisposal;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetDisposal.Command.CreateAssetDisposal
{
    public class CreateAssetDisposalCommandHandler : IRequestHandler<CreateAssetDisposalCommand, int>
    {
        private readonly IAssetDisposalCommandRepository _iassetdisposalcommandrepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public CreateAssetDisposalCommandHandler(IAssetDisposalCommandRepository iassetdisposalcommandrepository, IMediator imediator, IMapper imapper)
        {
            _iassetdisposalcommandrepository = iassetdisposalcommandrepository;
            _imediator = imediator;
            _imapper = imapper;
          
        }

        public async Task<int> Handle(CreateAssetDisposalCommand request, CancellationToken cancellationToken)
        {
            var assetDisposal = _imapper.Map<FAM.Domain.Entities.AssetMaster.AssetDisposal>(request);
            
            var result = await _iassetdisposalcommandrepository.CreateAsync(assetDisposal);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: assetDisposal.AssetId.ToString(),
                actionName: assetDisposal.Id.ToString(),
                details: $"AssetDisposal details was created",
                module: "AssetDisposal");
            await _imediator.Publish(domainEvent, cancellationToken);
            
            if (result > 0)
                  {
                     
                        return  result;
                 }
                 throw new Exception("AssetDisposal Creation Failed");
          
        }
    }
}