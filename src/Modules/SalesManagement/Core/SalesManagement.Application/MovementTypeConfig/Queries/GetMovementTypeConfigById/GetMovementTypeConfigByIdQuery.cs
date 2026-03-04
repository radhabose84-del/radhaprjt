using MediatR;
using SalesManagement.Application.MovementTypeConfig.Dto;

namespace SalesManagement.Application.MovementTypeConfig.Queries.GetMovementTypeConfigById
{
    public class GetMovementTypeConfigByIdQuery : IRequest<MovementTypeConfigDto?>
    {
        public int Id { get; set; }
    }
}
