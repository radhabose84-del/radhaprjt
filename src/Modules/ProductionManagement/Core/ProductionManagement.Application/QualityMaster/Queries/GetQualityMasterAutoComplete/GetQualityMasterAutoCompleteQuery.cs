using Contracts.Dtos.Lookups.Production;
using MediatR;

namespace ProductionManagement.Application.QualityMaster.Queries.GetQualityMasterAutoComplete
{
    public sealed record GetQualityMasterAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<QualityMasterLookupDto>>;
}
