using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.Modules.Queries.GetModules;
using MediatR;

namespace Core.Application.Modules.Queries.GetModuleAutoComplete
{
    public class GetModuleAutoCompleteQuery : IRequest<List<ModuleAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}