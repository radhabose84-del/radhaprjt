using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Command.UpdateActivityMster
{
    public class UpdateActivityMasterCommand  : IRequest<int>
    {

     public UpdateActivityMasterDto? UpdateActivityMaster  { get; set; }
    }
}