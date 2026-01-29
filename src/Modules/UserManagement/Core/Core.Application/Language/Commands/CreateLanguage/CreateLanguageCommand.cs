using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.Language.Queries.GetLanguages;
using MediatR;

namespace Core.Application.Language.Commands.CreateLanguage
{
    public class CreateLanguageCommand : IRequest<LanguageDTO>
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}