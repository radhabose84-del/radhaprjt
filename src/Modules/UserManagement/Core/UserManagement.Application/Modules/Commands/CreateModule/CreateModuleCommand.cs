using UserManagement.Application.Common.HttpResponse;
using MediatR;


namespace UserManagement.Application.Modules.Commands.CreateModule
{
    public class CreateModuleCommand  : IRequest<int>
    {
    public string? ModuleName { get; set; }
    // public List<string>? Menus { get; set; }
    }
}