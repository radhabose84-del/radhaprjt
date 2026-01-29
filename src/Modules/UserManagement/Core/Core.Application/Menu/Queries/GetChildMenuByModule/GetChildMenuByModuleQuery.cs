using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Menu.Queries.GetChildMenuByModule
{
    public class GetChildMenuByModuleQuery : IRequest<List<ChildMenuDTO>>
    {
        public List<int>? ParentId { get; set; }
    }
}