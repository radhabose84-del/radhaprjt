using Contracts.Common;
using Contracts.Dtos.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Gate;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Party;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Queries.GetPendingReferenceDocs
{
    public class GetPendingReferenceDocsQueryHandler
        : IRequestHandler<GetPendingReferenceDocsQuery, ApiResponseDTO<List<PendingReferenceDocDto>>>
    {
        private readonly IEnumerable<IPendingReferenceDocResolver> _resolvers;
        private readonly ITransactionTypeLookup _transactionTypeLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IPartyLookup _partyLookup;
        private readonly IMediator _mediator;

        public GetPendingReferenceDocsQueryHandler(
            IEnumerable<IPendingReferenceDocResolver> resolvers,
            ITransactionTypeLookup transactionTypeLookup,
            IIPAddressService ipAddressService,
            IPartyLookup partyLookup,
            IMediator mediator)
        {
            _resolvers = resolvers;
            _transactionTypeLookup = transactionTypeLookup;
            _ipAddressService = ipAddressService;
            _partyLookup = partyLookup;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<PendingReferenceDocDto>>> Handle(
            GetPendingReferenceDocsQuery request, CancellationToken cancellationToken)
        {
            // 1) Resolve the FE's TransactionTypeId → ShortName via Finance.TransactionTypeMaster (cached).
            //    Single source of truth — no hardcoded ids in resolvers, handler, or SQL.
            var txnTypes = await _transactionTypeLookup.GetByIdsAsync(new[] { request.ReferenceDocumentTypeId });
            var txnType = txnTypes.FirstOrDefault();
            if (txnType == null || string.IsNullOrWhiteSpace(txnType.ShortName))
            {
                return new ApiResponseDTO<List<PendingReferenceDocDto>>
                {
                    IsSuccess = false,
                    Message = $"Reference document type Id={request.ReferenceDocumentTypeId} not found in Finance.TransactionTypeMaster.",
                    Data = new List<PendingReferenceDocDto>()
                };
            }

            // 2) Dispatch to the resolver whose DocumentTypeCode matches that ShortName.
            var resolver = _resolvers.FirstOrDefault(r =>
                string.Equals(r.DocumentTypeCode, txnType.ShortName, StringComparison.OrdinalIgnoreCase));

            if (resolver == null)
            {
                return new ApiResponseDTO<List<PendingReferenceDocDto>>
                {
                    IsSuccess = false,
                    Message = $"No pending-document resolver is registered for DocumentTypeCode = '{txnType.ShortName}'.",
                    Data = new List<PendingReferenceDocDto>()
                };
            }

            // 3) Fetch the rows. Resolver SQL projects only the document-specific fields;
            //    TransactionTypeId / DocumentTypeCode are stamped here from the lookup row.
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var rows = (await resolver.GetPendingAsync(request.PartyId, unitId, cancellationToken)).ToList();

            foreach (var r in rows)
            {
                r.TransactionTypeId = txnType.Id;
                r.DocumentTypeCode = txnType.ShortName;
            }

            // 4) Cross-module name resolution (mirrors GRN's GetGateEntryPendingPo handler).
            var partyIds = rows.Select(r => r.PartyId).Where(id => id > 0).Distinct().ToList();
            if (partyIds.Count > 0)
            {
                var parties = await _partyLookup.GetByIdsAsync(partyIds, cancellationToken);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                foreach (var r in rows)
                {
                    if (partyDict.TryGetValue(r.PartyId, out var name))
                        r.PartyName = name;
                }
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetPendingReferenceDocsQuery",
                actionName: rows.Count.ToString(),
                details: $"Pending reference docs fetched (TxnType={txnType.Id}/{txnType.ShortName}, Party={request.PartyId}).",
                module: "GateInward");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<List<PendingReferenceDocDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = rows
            };
        }
    }
}
