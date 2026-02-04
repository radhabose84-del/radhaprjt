using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.Language.Commands.DeleteLanguage
{
    public class DeleteLanguageCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}