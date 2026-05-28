using MediatR;
using QCManagement.Application.QualitySpecification.Dto;

namespace QCManagement.Application.QualitySpecification.Queries.GetQualitySpecificationById
{
    public class GetQualitySpecificationByIdQuery : IRequest<QualitySpecificationDto?>
    {
        public int Id { get; set; }
    }
}
