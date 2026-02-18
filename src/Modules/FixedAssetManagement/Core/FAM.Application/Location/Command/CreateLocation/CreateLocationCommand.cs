using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using FAM.Application.Location.Queries.GetLocations;
using MediatR;

namespace FAM.Application.Location.Command.CreateLocation
{
    public class CreateLocationCommand : IRequest<LocationDto>
    {
        public string? Code { get; set; }
        public string? LocationName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }

    }
}