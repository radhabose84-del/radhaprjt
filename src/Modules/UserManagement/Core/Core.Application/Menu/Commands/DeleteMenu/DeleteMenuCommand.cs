using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Menu.Commands.DeleteMenu
{
    public class DeleteMenuCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}