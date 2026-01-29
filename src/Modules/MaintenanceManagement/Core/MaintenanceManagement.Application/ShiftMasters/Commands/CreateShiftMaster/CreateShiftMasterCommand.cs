using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasters.Commands.CreateShiftMaster
{
    public class CreateShiftMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public string ShiftCode { get; set; }
        public string ShiftName { get; set; }
        public DateOnly EffectiveDate { get; set; }
    }
}