using UserManagement.Application.Modules.Queries.GetModules;
using MediatR;

namespace UserManagement.Application.Modules.Queries.GetModuleAutoComplete
{
    public class GetModuleAutoCompleteQuery : IRequest<List<ModuleAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}