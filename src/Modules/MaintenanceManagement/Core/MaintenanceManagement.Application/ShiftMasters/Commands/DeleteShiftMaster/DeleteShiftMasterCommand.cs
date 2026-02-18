using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasters.Commands.DeleteShiftMaster
{
    public class DeleteShiftMasterCommand : IRequest<ApiResponseDTO<bool>>
    {
        public int Id { get; set; }
    }
}