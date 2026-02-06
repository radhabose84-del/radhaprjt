using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.Modules.Commands.DeleteModule
{
    public class DeleteModuleCommand : IRequest<bool>
    {
        public int ModuleId { get; set; }
    }
}