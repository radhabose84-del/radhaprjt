using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMasterById
{
    public class GetActivityCheckListMasterByIdQuery : IRequest<GetAllActivityCheckListMasterDto>
    {
        public int Id { get; set; }
    }
}