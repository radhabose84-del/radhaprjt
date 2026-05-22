using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.ServiceMaster.Commands.CreateService
{
    public class CreateServiceCommand : IRequest<GetServiceMasterDto>, IRequirePermission
    {
      public string? ServiceDescription { get; set; }
      public int SacId { get; set; }
      public  int UomId  { get; set; }
      public int? ServiceCategoryId { get; set; }
      public  byte IsActive { get; set; }

      public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
