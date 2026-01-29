using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetParentMaster
{
    public class GetAssetParentMasterQuery : IRequest<List<AssetMasterGeneralAutoCompleteDTO>>
    {
        public string? AssetType { get; set; }
    }
}