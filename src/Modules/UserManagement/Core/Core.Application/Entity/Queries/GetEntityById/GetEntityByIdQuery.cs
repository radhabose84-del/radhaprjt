using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Core.Application.Entity.Queries.GetEntity;
using MediatR;


namespace Core.Application.Entity.Queries.GetEntityById
{
    public class GetEntityByIdQuery :IRequest<GetEntityDTO>
    {
        public int EntityId { get; set; }
    }
}