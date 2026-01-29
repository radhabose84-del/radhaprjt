using FAM.Application.Common.HttpResponse;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.DepreciationGroup.Queries.GetAssetTypeQuery
{
   public class GetAssetTypeQuery : IRequest<ApiResponseDTO<List<GetMiscMasterDto>>> 
    {
        
    }
}