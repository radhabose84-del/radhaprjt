using FAM.Application.AssetLocation.Queries.GetAssetLocation;
using MediatR;

namespace FAM.Application.AssetLocation.Queries.GetAssetLocationById
{
    public class GetAssetLocationByIdQuery : IRequest<AssetLocationDto>
    {
         public int Id { get; set; }
         
       
    }
}