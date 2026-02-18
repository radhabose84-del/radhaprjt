using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.SwitchProfile.Commands.SwitchProfileByUnit
{
    public class SwitchProfileByUnitCommand : IRequest<SwitchProfileByUnitDTO>
    {
        public int UnitId { get; set; }
        public int CompanyId { get; set; }
        public int DivisionId { get; set; }
        public string? OldUnitId { get; set; }
    }
}