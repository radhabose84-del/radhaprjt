using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetTransferType
{
    public class GetTransferTypeQuery : IRequest<ApiResponseDTO<List<GetMiscMasterDto>>>
    {
      
        
    }
}