using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAllAssetTransfer
{
    public class GetAllTransferQuery : IRequest<List<GetAllTransferDtlDto>>
    
    {
       public int AssetTransferId  { get; set; }        
    }
}