using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetDtlToTransfer
{
    public class GetAssetDetailsToTransferQueryHandler  : IRequestHandler<GetAssetDetailsToTransferQuery, GetAssetDetailsToTransferHdrDto>
    {

      private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;   

      public GetAssetDetailsToTransferQueryHandler ( IAssetTransferQueryRepository assetTransferQueryRepository)
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;
        }
         public async Task<GetAssetDetailsToTransferHdrDto> Handle(GetAssetDetailsToTransferQuery request, CancellationToken cancellationToken)
        {
            // Fetch asset details from the repository
            var assetTransferHdr = await _assetTransferQueryRepository.GetAssetDetailsToTransferByIdAsync(request.AssetId);

            if (assetTransferHdr == null)
            {
                throw new ValidationException($"Asset Transfer details not found for Asset ID {request.AssetId}.");
               
            }

            return assetTransferHdr;
        }
       
    }
}