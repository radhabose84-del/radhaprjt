using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Core.Application.Entity.Queries.GetEntity;
using MediatR;


namespace Core.Application.Entity.Commands.UpdateEntity
{
    public class UpdateEntityCommand : IRequest<int>
    {
    public int Id { get; set; }
    public string? EntityName { get; set; }
    public string? EntityDescription { get; set; }
    public string? Address { get; set; }
    public string? Phone  { get; set; }
    public string? Email { get; set; }
    public byte IsActive { get; set; } 
       
    }
}
