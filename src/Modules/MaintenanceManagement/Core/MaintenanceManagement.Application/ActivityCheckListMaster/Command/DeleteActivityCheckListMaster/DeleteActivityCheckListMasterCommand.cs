using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Command.DeleteActivityCheckListMaster
{
    public class DeleteActivityCheckListMasterCommand : IRequest<bool> 
    {
          public int Id { get; set; }
    }
}