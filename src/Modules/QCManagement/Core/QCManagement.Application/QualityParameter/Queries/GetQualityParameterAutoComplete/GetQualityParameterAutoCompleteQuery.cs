using MediatR;
using QCManagement.Application.QualityParameter.Dto;

namespace QCManagement.Application.QualityParameter.Queries.GetQualityParameterAutoComplete
{
    public sealed record GetQualityParameterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<QualityParameterLookupDto>>;
}
