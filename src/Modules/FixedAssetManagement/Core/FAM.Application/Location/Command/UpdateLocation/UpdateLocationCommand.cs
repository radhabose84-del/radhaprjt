using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.Location.Command.UpdateLocation
{
    public class UpdateLocationCommand: IRequest<bool>
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? LocationName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public byte IsActive { get; set; }
    }
}