using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Entity.Queries.GetEntity;
using MediatR;

namespace UserManagement.Application.Entity.Commands.CreateEntity
{
    public class CreateEntityCommand :IRequest<int>
    {

    public string? EntityName { get; set; }
    public string? EntityDescription { get; set; }
    public string? Address { get; set; }
    public string? Phone  { get; set; }
    public string? Email { get; set; }
   
    }
}