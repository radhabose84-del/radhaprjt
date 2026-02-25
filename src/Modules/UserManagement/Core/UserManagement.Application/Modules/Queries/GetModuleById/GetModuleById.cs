using UserManagement.Application.Modules.Queries.GetModules;
using MediatR;

namespace UserManagement.Application.Modules.Queries.GetModuleById
{
    public class GetModuleByIdQuery: IRequest<ModuleByIdDto>
    {
        public int Id { get; set; }
        
    }
}