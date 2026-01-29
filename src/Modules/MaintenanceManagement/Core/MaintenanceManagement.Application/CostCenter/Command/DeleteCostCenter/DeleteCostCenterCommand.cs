using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.CostCenter.Command.DeleteCostCenter
{
    public class DeleteCostCenterCommand : IRequest<int> 
    {
        public int Id { get; set; }
    }
}