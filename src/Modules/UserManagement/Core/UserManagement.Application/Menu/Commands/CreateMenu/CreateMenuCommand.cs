using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Menu.Commands.CreateMenu
{
    public class CreateMenuCommand : IRequest<int>
    {
        public string MenuName { get; set; } = default!;
        public int ModuleId { get; set; }
        public string? MenuIcon { get; set; }
        public string MenuUrl { get; set; } = default!;
        public int ParentId { get; set; }
        public int SortOrder { get; set; }
        public string? Type { get; set; }
    }
}