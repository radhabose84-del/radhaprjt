using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMasterById
{
    public class GetMachineMasterByIdQuery : IRequest<MachineMasterDto>
    {
        public int Id { get; set; }
    }
}