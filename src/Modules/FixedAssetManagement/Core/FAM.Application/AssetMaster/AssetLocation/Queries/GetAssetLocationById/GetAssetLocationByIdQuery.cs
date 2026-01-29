using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetLocation.Queries.GetAssetLocation;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetLocation.Queries.GetAssetLocationById
{
    public class GetAssetLocationByIdQuery : IRequest<AssetLocationDto>
    {
         public int Id { get; set; }
         
       
    }
}