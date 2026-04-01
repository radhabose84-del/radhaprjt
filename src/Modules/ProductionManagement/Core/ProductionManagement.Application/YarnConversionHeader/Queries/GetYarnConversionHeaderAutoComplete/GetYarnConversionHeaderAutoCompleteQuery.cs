using MediatR;
using ProductionManagement.Application.YarnConversionHeader.Dto;

namespace ProductionManagement.Application.YarnConversionHeader.Queries.GetYarnConversionHeaderAutoComplete
{
    public sealed record GetYarnConversionHeaderAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<YarnConversionHeaderLookupDto>>;
}
