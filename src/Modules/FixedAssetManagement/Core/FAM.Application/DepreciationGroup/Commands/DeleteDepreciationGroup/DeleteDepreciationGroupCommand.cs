using FAM.Application.Common.HttpResponse;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using MediatR;

namespace FAM.Application.DepreciationGroup.Commands.DeleteDepreciationGroup
{
    public class DeleteDepreciationGroupCommand :  IRequest<DepreciationGroupDTO>
    {
          public int Id { get; set; }         
    }
}