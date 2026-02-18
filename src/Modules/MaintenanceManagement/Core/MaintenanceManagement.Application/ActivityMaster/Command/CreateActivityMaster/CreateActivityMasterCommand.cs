using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetAllActivityMaster;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Command.CreateActivityMaster
{
    public class CreateActivityMasterCommand : IRequest<int>
    {

        public CreateActivityMasterDto? CreateActivityMasterDto { get; set; }
        


    }
}