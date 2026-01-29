using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Divisions.Commands.UpdateDivision
{
    public class UpdateDivisionCommand : IRequest<bool>
    {
         public int Id { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public byte IsActive { get; set; }
    }
}