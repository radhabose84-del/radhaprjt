using FAM.Application.Manufacture.Queries.GetManufacture;
using MediatR;

namespace FAM.Application.Manufacture.Queries.GetManufactureById
{
    public class GetManufactureByIdQuery : IRequest<ManufactureDTO>
    {
        public int Id { get; set; }
    }
}