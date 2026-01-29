using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.DepreciationDetail.Queries.GetDepreciationMethod
{
    public class GetDepreciationMethodQuery  : IRequest<ApiResponseDTO<List<GetMiscMasterDto>>> 
    {
        
    }
}