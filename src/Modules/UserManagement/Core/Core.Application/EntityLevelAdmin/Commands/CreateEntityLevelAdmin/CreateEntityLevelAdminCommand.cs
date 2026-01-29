using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.EntityLevelAdmin.Commands.CreateEntityLevelAdmin
{
    public class CreateEntityLevelAdminCommand : IRequest<int>
    {
        public string? Email { get; set; }
        public int EntityId { get; set; }
        public int CompanyId { get; set; }
    }
}