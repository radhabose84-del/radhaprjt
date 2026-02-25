using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using MediatR;

namespace FAM.Application.AssetSubGroup.Queries.GetAssetSubGroupById
{
    public class GetAssetSubGroupByIdQuery : IRequest<AssetSubGroupDto>
    {        
        public int Id { get; set; }
    }
}