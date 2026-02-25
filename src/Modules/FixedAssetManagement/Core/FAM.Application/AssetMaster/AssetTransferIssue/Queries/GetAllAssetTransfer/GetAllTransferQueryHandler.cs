using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAllAssetTransfer
{

    public class GetAllTransferQueryHandler : IRequestHandler<GetAllTransferQuery, List<GetAllTransferDtlDto>>
    {
        private readonly IAssetTransferQueryRepository _assetTransferQueryRepository; 
        
        public GetAllTransferQueryHandler(IAssetTransferQueryRepository assetTransferQueryRepository)
            {
                _assetTransferQueryRepository = assetTransferQueryRepository;
            }        

      public async Task<List<GetAllTransferDtlDto>> Handle(GetAllTransferQuery request, CancellationToken cancellationToken)
        {
            // Fetch asset details from the repository
            var assetTransferHdr = await _assetTransferQueryRepository.GetAssetTransferByIDAsync(request.AssetTransferId);

            if (assetTransferHdr == null || !assetTransferHdr.Any())
            {
                throw new ValidationException("Asset Transfer details not found");
                
            }

            return  assetTransferHdr;
        }
    }
}