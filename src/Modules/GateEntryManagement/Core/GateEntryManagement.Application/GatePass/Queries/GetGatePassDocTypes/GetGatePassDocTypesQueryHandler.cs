using Contracts.Dtos.Lookups.Finance;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GatePass.Queries.GetGatePassDocTypes
{
    public class GetGatePassDocTypesQueryHandler : IRequestHandler<GetGatePassDocTypesQuery, List<TransactionTypeLookupDto>>
    {
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetGatePassDocTypesQueryHandler(
            IDocumentSequenceLookup documentSequenceLookup,
            IFinancialYearLookup financialYearLookup,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _documentSequenceLookup = documentSequenceLookup;
            _financialYearLookup = financialYearLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<List<TransactionTypeLookupDto>> Handle(GetGatePassDocTypesQuery request, CancellationToken cancellationToken)
        {
            // Get current financial year based on today's date
            var allYears = await _financialYearLookup.GetAllFinancialYearAsync();
            var today = DateTime.Today;
            var currentYear = allYears.FirstOrDefault(y => y.IsActive && today >= y.StartDate && today <= y.EndDate);

            if (currentYear == null)
                return new List<TransactionTypeLookupDto>();

            var unitId = _ipAddressService.GetUnitId() ?? 0;

            // Cross-module call via Finance lookup — no direct SQL
            var data = await _documentSequenceLookup.GetTransactionTypesForYearAsync(
                currentYear.FinancialYearId, unitId, request.ModuleId, request.MenuId, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetGatePassDocTypesQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "GatePass document types were fetched.",
                module: "GatePass"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return data;
        }
    }
}
