using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Menu.Queries.GetParentMenu
{
    public class GetParentMenuQuery : IRequest<List<ParentMenuDto>>
    {
        public string? SearchPattern { get; set; }
    }
}