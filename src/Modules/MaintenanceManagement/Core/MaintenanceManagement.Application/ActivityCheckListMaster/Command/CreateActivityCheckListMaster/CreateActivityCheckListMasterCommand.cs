using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Command.CreateActivityCheckListMaster
{
    public class CreateActivityCheckListMasterCommand : IRequest<int>
    {

        public int ActivityID { get; set; }
        public string? ActivityCheckList { get; set; }       
        public int  UnitId { get; set; }

    }
}