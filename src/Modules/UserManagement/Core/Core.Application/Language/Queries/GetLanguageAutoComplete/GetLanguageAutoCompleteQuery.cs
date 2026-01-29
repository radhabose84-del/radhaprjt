using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.Language.Queries.GetLanguages;
using MediatR;

namespace Core.Application.Language.Queries.GetLanguageAutoComplete
{
    public class GetLanguageAutoCompleteQuery : IRequest<List<LanguageAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}