using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Modules.Commands.DeleteModule
{
    public class DeleteModuleCommand : IRequest<bool>
    {
        public int ModuleId { get; set; }
    }
}