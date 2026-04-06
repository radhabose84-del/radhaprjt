using MediatR;
using ProductionManagement.Application.YarnConversionHeader.Dto;

namespace ProductionManagement.Application.YarnConversionHeader.Queries.GetYarnConversionHeaderById
{
    public class GetYarnConversionHeaderByIdQuery : IRequest<YarnConversionHeaderDto?>
    {
        public int Id { get; set; }
    }
}
