using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetSubGroup.Queries.GetAssetGroupById
{
    public class GetGroupByIdQuery : IRequest<List<AssetSubGroupDto>>
    {
        public int GroupId { get; set; }        
    }
}