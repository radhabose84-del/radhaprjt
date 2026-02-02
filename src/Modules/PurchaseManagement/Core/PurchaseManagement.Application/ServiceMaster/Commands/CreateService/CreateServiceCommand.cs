using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using MediatR;

namespace PurchaseManagement.Application.ServiceMaster.Commands.CreateService
{
    public class CreateServiceCommand : IRequest<GetServiceMasterDto>
    {
      public string? ServiceDescription { get; set; }
      public int SacId { get; set; }
      public  int UomId  { get; set; }
      public int? ServiceCategoryId { get; set; }
      public  byte IsActive { get; set; }

    }
}