using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.CostCenter.Command.DeleteCostCenter
{
    public class DeleteCostCenterCommand : IRequest<int> 
    {
        public int Id { get; set; }
    }
}