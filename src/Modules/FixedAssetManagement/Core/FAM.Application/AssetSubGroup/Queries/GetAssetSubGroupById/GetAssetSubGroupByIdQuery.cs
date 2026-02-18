using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetSubGroup.Queries.GetAssetSubGroupById
{
    public class GetAssetSubGroupByIdQuery : IRequest<AssetSubGroupDto>
    {        
        public int Id { get; set; }
    }
}