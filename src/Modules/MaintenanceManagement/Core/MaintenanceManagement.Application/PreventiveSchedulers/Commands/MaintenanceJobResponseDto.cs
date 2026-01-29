using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands
{
    public class MaintenanceJobResponseDto
    {
        public string Message { get; set; }
         public string JobId { get; set; }
    }
}