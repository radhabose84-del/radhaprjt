using Core.Application.Common.HttpResponse;
using MediatR;


namespace Core.Application.Modules.Commands.CreateModule
{
    public class CreateModuleCommand  : IRequest<int>
    {
    public string? ModuleName { get; set; }
    // public List<string>? Menus { get; set; }
    }
}