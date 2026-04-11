using MediatR;
using ProductionManagement.Application.RawMaterialType.Dto;

namespace ProductionManagement.Application.RawMaterialType.Queries.GetRawMaterialTypeById
{
    public class GetRawMaterialTypeByIdQuery : IRequest<RawMaterialTypeDto>
    {
        public int Id { get; set; }
    }
}
