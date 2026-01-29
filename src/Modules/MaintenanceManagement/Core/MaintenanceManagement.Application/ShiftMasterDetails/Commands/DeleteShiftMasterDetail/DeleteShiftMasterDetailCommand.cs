using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasterDetails.Commands.DeleteShiftMasterDetail
{
    public class DeleteShiftMasterDetailCommand : IRequest<ApiResponseDTO<bool>>
    {
        public int Id { get; set; }
    }
}