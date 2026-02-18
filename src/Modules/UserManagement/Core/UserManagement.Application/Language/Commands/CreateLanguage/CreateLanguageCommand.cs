using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Application.Language.Queries.GetLanguages;
using MediatR;

namespace UserManagement.Application.Language.Commands.CreateLanguage
{
    public class CreateLanguageCommand : IRequest<LanguageDTO>
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
}