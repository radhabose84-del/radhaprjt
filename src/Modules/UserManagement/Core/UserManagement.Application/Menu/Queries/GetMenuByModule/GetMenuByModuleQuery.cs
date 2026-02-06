using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.Menu.Queries.GetMenuByModule
{
    public class GetMenuByModuleQuery : IRequest<List<MenuDTO>>
    {
        public List<int>? ModuleId { get; set; }
    }
}