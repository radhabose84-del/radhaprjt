using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.DeletePreventiveScheduler
{
    public class DeletePreventiveSchedulerCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}