using FAM.Application.Common.HttpResponse;
using MediatR;


namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetails
{
    public class GetAssetReceiptDetailsQuery :  IRequest<ApiResponseDTO<List<AssetReceiptDetailsDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; } 
        public string? SearchTerm { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}