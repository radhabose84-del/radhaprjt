using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceTypeById
{
    public class GetMaintenanceTypeByIdQuery : IRequest<MaintenanceTypeDto>
    {
        public int Id { get; set; }
    }
}