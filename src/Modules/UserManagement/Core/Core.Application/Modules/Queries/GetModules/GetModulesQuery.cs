using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Modules.Queries.GetModules
{
    public class GetModulesQuery: IRequest<ApiResponseDTO<List<ModuleDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }

    // public class ModuleDto
    // {
    //     public string ModuleName { get; set; }
    //     public string IsDeleted { get; set; }

    //     public List<string> Menus { get; set; }
    // }
}