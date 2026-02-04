using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.EntityLevelAdmin.Commands.CreateEntityLevelAdmin
{
    public class CreateEntityLevelAdminCommand : IRequest<int>
    {
        public string? Email { get; set; }
        public int EntityId { get; set; }
        public int CompanyId { get; set; }
    }
}