using UserManagement.Application.Common;
using Contracts.Common;
using UserManagement.Application.Entity.Queries.GetEntity;
using MediatR;

namespace UserManagement.Application.Entity.Commands.DeleteEntity
{
    public class DeleteEntityCommand : IRequest<int>
    {
        public int EntityId { get; set; }
    
    }
}