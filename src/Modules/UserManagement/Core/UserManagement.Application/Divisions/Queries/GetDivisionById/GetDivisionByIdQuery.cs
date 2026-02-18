using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using System.Text;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using Contracts.Common;

namespace UserManagement.Application.Divisions.Queries.GetDivisionById
{
    public class GetDivisionByIdQuery : IRequest<DivisionDTO>
    {
        public int Id { get; set; }
    }
}