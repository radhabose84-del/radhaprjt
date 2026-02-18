using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Menu.Queries.GetMenuByModule
{
    public class GetMenuByModuleQuery : IRequest<List<MenuDTO>>
    {
        public List<int>? ModuleId { get; set; }
    }
}