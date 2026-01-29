using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceType.Command.CreateMaintenanceType
{
    public class CreateMaintenanceTypeCommand :IRequest<int>
    {
         public string? TypeName { get; set; }
    }
}