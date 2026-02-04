using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Entity.Queries.GetEntity;
using MediatR;

namespace UserManagement.Application.Entity.Commands.DeleteEntity
{
    public class DeleteEntityCommand : IRequest<int>
    {
        public int EntityId { get; set; }
    
    }
}