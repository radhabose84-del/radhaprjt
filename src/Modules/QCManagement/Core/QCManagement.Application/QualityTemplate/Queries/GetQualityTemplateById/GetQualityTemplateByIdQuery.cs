using MediatR;
using QCManagement.Application.QualityTemplate.Dto;

namespace QCManagement.Application.QualityTemplate.Queries.GetQualityTemplateById
{
    public class GetQualityTemplateByIdQuery : IRequest<QualityTemplateDto?>
    {
        public int Id { get; set; }
    }
}
