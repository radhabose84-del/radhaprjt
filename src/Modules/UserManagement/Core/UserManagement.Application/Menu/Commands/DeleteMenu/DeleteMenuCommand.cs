using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Menu.Commands.DeleteMenu
{
    public class DeleteMenuCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}