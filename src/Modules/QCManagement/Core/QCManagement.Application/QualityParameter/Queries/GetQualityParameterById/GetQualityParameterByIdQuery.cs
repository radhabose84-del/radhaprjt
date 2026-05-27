using MediatR;
using QCManagement.Application.QualityParameter.Dto;

namespace QCManagement.Application.QualityParameter.Queries.GetQualityParameterById
{
    public class GetQualityParameterByIdQuery : IRequest<QualityParameterDto?>
    {
        public int Id { get; set; }
    }
}
