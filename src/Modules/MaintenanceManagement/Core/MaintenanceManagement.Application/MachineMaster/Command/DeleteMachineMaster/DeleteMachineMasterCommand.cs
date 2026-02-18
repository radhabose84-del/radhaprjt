using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Command.DeleteMachineMaster
{
    public class DeleteMachineMasterCommand : IRequest<bool>
    {
        public int Id { get; set; } 
    }
}