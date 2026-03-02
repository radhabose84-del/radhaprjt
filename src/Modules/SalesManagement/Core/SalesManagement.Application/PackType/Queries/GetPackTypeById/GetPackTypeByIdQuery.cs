using MediatR;
using SalesManagement.Application.PackType.Dto;

namespace SalesManagement.Application.PackType.Queries.GetPackTypeById
{
    public class GetPackTypeByIdQuery : IRequest<PackTypeDto?>
    {
        public int Id { get; set; }
    }
}
