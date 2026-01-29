using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetUnMappedMachine
{
    public class UnMappedMachineDto
    {
        public int Id { get; set; }
        public string MachineCode { get; set; }
        public string MachineName { get; set; }
    }
}