using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MRS.Queries.GetSubCostCenter
{
    public class GetSubCostCenterQuery : IRequest<List<MSubCostCenterDto>>
    {
         public string? OldUnitcode { get; set; }
    }
}