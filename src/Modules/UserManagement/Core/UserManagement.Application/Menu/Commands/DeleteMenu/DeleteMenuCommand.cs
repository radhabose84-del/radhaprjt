using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.Menu.Commands.DeleteMenu
{
    public class DeleteMenuCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}