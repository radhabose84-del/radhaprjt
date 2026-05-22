using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNPutaway
{
    public class CreateGRNPutawayCommand : IRequest<int>, IRequirePermission
    {
          public List<CreateGRNPutawayDto>? PutawayList { get; set; } 
          public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
