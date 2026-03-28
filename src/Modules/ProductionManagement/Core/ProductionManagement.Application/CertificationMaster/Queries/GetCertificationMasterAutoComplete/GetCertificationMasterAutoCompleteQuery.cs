using Contracts.Dtos.Lookups.Production;
using MediatR;

namespace ProductionManagement.Application.CertificationMaster.Queries.GetCertificationMasterAutoComplete
{
    public sealed record GetCertificationMasterAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<CertificationMasterLookupDto>>;
}
