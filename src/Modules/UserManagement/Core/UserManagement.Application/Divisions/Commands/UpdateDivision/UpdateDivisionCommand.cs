using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Divisions.Commands.UpdateDivision
{
    public class UpdateDivisionCommand : IRequest<bool>
    {
         public int Id { get; set; }
        public string ShortName { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int CompanyId { get; set; }
        public byte IsActive { get; set; }
    }
}