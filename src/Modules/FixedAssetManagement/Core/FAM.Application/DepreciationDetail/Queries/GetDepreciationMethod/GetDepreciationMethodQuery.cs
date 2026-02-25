using Contracts.Common;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.DepreciationDetail.Queries.GetDepreciationMethod
{
    public class GetDepreciationMethodQuery  : IRequest<ApiResponseDTO<List<GetMiscMasterDto>>> 
    {
        
    }
}