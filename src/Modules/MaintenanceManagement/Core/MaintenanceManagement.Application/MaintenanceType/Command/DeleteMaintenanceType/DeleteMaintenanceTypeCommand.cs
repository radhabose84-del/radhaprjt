using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceType.Command.DeleteMaintenanceType
{
    public class DeleteMaintenanceTypeCommand : IRequest<int>
    {
        public int Id { get; set; } 
    }
}