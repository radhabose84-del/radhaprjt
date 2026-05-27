using MediatR;
using QCManagement.Application.QualityTemplate.Dto;

namespace QCManagement.Application.QualityTemplate.Queries.GetQualityTemplateAutoComplete
{
    public sealed record GetQualityTemplateAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<QualityTemplateLookupDto>>;
}
