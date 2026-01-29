using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using System.Text;
using Core.Application.Divisions.Queries.GetDivisions;
using Core.Application.Common.HttpResponse;

namespace Core.Application.Divisions.Queries.GetDivisionById
{
    public class GetDivisionByIdQuery : IRequest<DivisionDTO>
    {
        public int Id { get; set; }
    }
}