using MediatR;

namespace FAM.Application.AssetMaster.AssetLocation.Queries.GetSubLocationById
{
    public class GetSubLocationByIdQuery : IRequest<List<GetAssetSubLocationDto>>
    {
         public int Id { get; set; }
    }
}