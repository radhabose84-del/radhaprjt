using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Menu.Queries.GetChildMenuByModule
{
    public class GetChildMenuByModuleQuery : IRequest<List<ChildMenuDTO>>
    {
        public List<int>? ParentId { get; set; }
    }
}