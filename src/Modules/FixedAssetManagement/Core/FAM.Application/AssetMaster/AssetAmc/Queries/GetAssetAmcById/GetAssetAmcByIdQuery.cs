using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmc;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmcById
{
    public class GetAssetAmcByIdQuery : IRequest<AssetAmcDto>
    {
        public int Id {get; set;}
    }
}