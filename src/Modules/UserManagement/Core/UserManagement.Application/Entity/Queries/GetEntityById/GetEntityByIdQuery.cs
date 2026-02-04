using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Entity.Queries.GetEntity;
using MediatR;


namespace UserManagement.Application.Entity.Queries.GetEntityById
{
    public class GetEntityByIdQuery :IRequest<GetEntityDTO>
    {
        public int EntityId { get; set; }
    }
}