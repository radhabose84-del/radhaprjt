using FAM.Application.Common.HttpResponse;
using FAM.Application.Manufacture.Queries.GetManufacture;
using MediatR;

namespace FAM.Application.Manufacture.Commands.DeleteManufacture
{
    public class DeleteManufactureCommand :  IRequest<ManufactureDTO>
    {
        public int Id { get; set; }  
    }
}