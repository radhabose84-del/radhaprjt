using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMaster;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMasterById
{
    public class GetShiftMasterByIdQuery : IRequest<ApiResponseDTO<ShiftMasterDTO>>
    {
        public int Id { get; set; }
    }
}