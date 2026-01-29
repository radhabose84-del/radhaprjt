using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetLocation.Queries.GetSubLocationById
{
    public class GetSubLocationByIdQuery : IRequest<List<GetAssetSubLocationDto>>
    {
         public int Id { get; set; }
    }
}