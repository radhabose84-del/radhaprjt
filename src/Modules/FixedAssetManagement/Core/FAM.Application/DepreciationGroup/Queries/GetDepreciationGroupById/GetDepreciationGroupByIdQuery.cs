using Contracts.Common;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using MediatR;

namespace FAM.Application.DepreciationGroup.Queries.GetDepreciationGroupById
{
    public class GetDepreciationGroupByIdQuery : IRequest<DepreciationGroupDTO>
    {
        public int Id { get; set; }
    }
}