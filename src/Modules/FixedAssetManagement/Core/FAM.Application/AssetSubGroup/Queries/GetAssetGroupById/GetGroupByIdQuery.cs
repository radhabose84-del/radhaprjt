using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetSubGroup.Queries.GetAssetGroupById
{
    public class GetGroupByIdQuery : IRequest<List<AssetSubGroupDto>>
    {
        public int GroupId { get; set; }        
    }
}