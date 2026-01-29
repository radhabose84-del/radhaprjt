using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Menu.Commands.CreateMenu
{
    public class CreateMenuCommand : IRequest<int>
    {
        public string MenuName { get; set; }
        public int ModuleId { get; set; }
        public string? MenuIcon { get; set; }
        public string MenuUrl { get; set; }
        public int ParentId { get; set; }
        public int SortOrder { get; set; }
        public string? Type { get; set; }
    }
}