using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationAutoComplete
{
    public class GetAssetSpecificationAutoCompleteQuery : IRequest<List<AssetSpecificationJsonDto>>
    {
        public string? SearchPattern { get; set; }
    }
}