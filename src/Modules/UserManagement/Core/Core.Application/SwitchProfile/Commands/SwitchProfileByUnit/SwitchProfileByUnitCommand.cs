using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.SwitchProfile.Commands.SwitchProfileByUnit
{
    public class SwitchProfileByUnitCommand : IRequest<SwitchProfileByUnitDTO>
    {
        public int UnitId { get; set; }
        public int CompanyId { get; set; }
        public int DivisionId { get; set; }
        public string? OldUnitId { get; set; }
    }
}