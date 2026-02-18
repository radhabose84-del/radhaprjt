using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetSourceAutoComplete
{
    public class GetAssetSourceAutoCompleteQuery  : IRequest<List<AssetSourceAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}