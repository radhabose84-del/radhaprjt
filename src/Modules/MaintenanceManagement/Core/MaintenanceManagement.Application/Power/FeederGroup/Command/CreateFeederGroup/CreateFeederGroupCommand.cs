using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.Power.FeederGroup.Command.CreateFeederGroup
{
    public class CreateFeederGroupCommand : IRequest<int>
    {
        public string? FeederGroupCode { get; set; }
        public string? FeederGroupName { get; set; }
        public int UnitId { get; set; }
       
    }
}