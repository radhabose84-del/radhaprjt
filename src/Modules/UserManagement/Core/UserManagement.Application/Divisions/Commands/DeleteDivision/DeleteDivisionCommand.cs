using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using System.Reflection;
using System.Text;
using Contracts.Common;
using UserManagement.Application.Divisions.Queries.GetDivisions;

namespace UserManagement.Application.Divisions.Commands.DeleteDivision
{
    public class DeleteDivisionCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}