using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Application.Menu.Commands.UploadMenu
{
    public class UploadMenuCommand : IRequest<string>
    {
        public IFormFile File { get; set; } = default!;
    }
}