using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Menu.Queries.GetParentMenu
{
    public class GetParentMenuQuery : IRequest<List<ParentMenuDto>>
    {
        public string? SearchPattern { get; set; }
    }
}