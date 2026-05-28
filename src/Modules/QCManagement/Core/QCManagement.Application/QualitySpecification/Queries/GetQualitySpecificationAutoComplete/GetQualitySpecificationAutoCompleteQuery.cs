using MediatR;
using QCManagement.Application.QualitySpecification.Dto;

namespace QCManagement.Application.QualitySpecification.Queries.GetQualitySpecificationAutoComplete
{
    public sealed record GetQualitySpecificationAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<QualitySpecificationLookupDto>>;
}
