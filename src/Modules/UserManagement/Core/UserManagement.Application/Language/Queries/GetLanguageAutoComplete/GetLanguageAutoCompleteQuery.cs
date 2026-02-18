using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Application.Language.Queries.GetLanguages;
using MediatR;

namespace UserManagement.Application.Language.Queries.GetLanguageAutoComplete
{
    public class GetLanguageAutoCompleteQuery : IRequest<List<LanguageAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}