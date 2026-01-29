using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using System.Reflection;
using System.Text;
using Core.Application.Common.HttpResponse;
using Core.Application.Divisions.Queries.GetDivisions;

namespace Core.Application.Divisions.Commands.DeleteDivision
{
    public class DeleteDivisionCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}