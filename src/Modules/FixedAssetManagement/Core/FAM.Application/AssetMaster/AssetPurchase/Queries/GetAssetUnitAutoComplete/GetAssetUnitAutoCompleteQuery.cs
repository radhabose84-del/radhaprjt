using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetSourceAutoComplete
{
    public class GetAssetUnitAutoCompleteQuery : IRequest<List<AssetUnitAutoCompleteDto>>
    {
        public string? Username { get; set; }
    }
}