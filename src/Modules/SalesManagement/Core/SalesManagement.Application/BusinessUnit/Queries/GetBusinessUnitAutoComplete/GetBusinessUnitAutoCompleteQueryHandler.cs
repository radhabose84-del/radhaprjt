
using MediatR;
using SalesManagement.Application.BusinessUnit.Dto;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;

namespace SalesManagement.Application.BusinessUnit.Queries.GetBusinessUnitAutoComplete
{
    public class GetBusinessUnitAutoCompleteQueryHandler : IRequestHandler<GetBusinessUnitAutoCompleteQuery, IReadOnlyList<BusinessUnitLookupDto>>
    {
        private readonly IBusinessUnitQueryRepository _queryRepository;

        public GetBusinessUnitAutoCompleteQueryHandler(IBusinessUnitQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<IReadOnlyList<BusinessUnitLookupDto>> Handle(GetBusinessUnitAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.AutocompleteAsync(request.Term, cancellationToken);
        }
    }
}
