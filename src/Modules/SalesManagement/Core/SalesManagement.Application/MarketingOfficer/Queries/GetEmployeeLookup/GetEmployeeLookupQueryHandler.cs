using FluentValidation;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Dto;

namespace SalesManagement.Application.MarketingOfficer.Queries.GetEmployeeLookup
{
    public class GetEmployeeLookupQueryHandler : IRequestHandler<GetEmployeeLookupQuery, List<EmployeeLookupDto>>
    {
        private readonly IMarketingOfficerQueryRepository _queryRepository;

        public GetEmployeeLookupQueryHandler(IMarketingOfficerQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<List<EmployeeLookupDto>> Handle(GetEmployeeLookupQuery request, CancellationToken cancellationToken)
        {
            var oldUnitId = request.OldUnitId;

            if (string.IsNullOrWhiteSpace(oldUnitId))
            {
                throw new ValidationException("OldUnitId not found in token.");
            }

            var result = await _queryRepository.GetEmployeeLookupAsync(oldUnitId, request.EmpNo);
            return result;
        }
    }
}
