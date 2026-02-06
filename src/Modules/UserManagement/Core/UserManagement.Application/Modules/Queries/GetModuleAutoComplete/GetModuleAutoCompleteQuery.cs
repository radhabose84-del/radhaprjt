using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Modules.Queries.GetModules;
using MediatR;

namespace UserManagement.Application.Modules.Queries.GetModuleAutoComplete
{
    public class GetModuleAutoCompleteQuery : IRequest<List<ModuleAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}