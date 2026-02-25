using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetIssueDetailsById
{
    public class GetAssetIssueDetailsByIdQueryHandler : IRequestHandler<GetAssetIssueDetailsByIdQuery, ApiResponseDTO<AssetTransferJsonDto>>
    {
        private readonly IAssetTransferReceiptQueryRepository _assetTransferQueryRepository;  
        public GetAssetIssueDetailsByIdQueryHandler ( IAssetTransferReceiptQueryRepository assetTransferQueryRepository)
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;
        }

        public Task<ApiResponseDTO<AssetTransferJsonDto>> Handle(GetAssetIssueDetailsByIdQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // public async Task<ApiResponseDTO<AssetTransferJsonDto>> Handle(GetAssetIssueDetailsByIdQuery request, CancellationToken cancellationToken)
        // {
        //     var assetTransfer = await _assetTransferQueryRepository.GetAssetTransferByIdAsync(request.AssetTransferId);

        //     if (assetTransfer == null)
        //     {
        //         return new ApiResponseDTO<AssetTransferJsonDto>
        //         {
        //             IsSuccess = false,
        //             Message = $"Asset Transfer Issue with ID {request.AssetTransferId} not found."

        //         };
        //     }
        //         return new ApiResponseDTO<AssetTransferJsonDto>
        //         {
        //             IsSuccess = true,
        //             Message = "Asset Transfer retrieved successfully.",
        //             Data = assetTransfer
        //         };    
        // }
    }
}