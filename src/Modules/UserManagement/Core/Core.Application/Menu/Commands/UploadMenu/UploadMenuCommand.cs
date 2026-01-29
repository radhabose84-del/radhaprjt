using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Core.Application.Menu.Commands.UploadMenu
{
    public class UploadMenuCommand : IRequest<string>
    {
        public IFormFile File { get; set; }
    }
}