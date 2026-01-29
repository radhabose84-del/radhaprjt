using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Modules.Commands.UpdateModule
{
    public class UpdateModuleCommand : IRequest<bool>
    {
    public int ModuleId { get; set; }
    public string? ModuleName { get; set; }
    // public List<string>? Menus { get; set; }
    }
}