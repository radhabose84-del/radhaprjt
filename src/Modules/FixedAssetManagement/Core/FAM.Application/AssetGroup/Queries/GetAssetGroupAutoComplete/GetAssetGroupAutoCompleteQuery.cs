using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetGroup.Queries.GetAssetGroupAutoComplete
{
    public class GetAssetGroupAutoCompleteQuery : IRequest<List<AssetGroupAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
       // public int AssetGroupId { get; set; }
    }
}