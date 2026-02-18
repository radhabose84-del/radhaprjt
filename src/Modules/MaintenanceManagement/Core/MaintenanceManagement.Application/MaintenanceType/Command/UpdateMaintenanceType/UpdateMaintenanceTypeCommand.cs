using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceType.Command.UpdateMaintenanceType
{
    public class UpdateMaintenanceTypeCommand :IRequest<int>
    {
        public int Id {get;set;}
        public string? TypeName { get; set; }
        public byte IsActive { get; set; }
    }
}