using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.DeletePreventiveScheduler
{
    public class DeletePreventiveSchedulerCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}