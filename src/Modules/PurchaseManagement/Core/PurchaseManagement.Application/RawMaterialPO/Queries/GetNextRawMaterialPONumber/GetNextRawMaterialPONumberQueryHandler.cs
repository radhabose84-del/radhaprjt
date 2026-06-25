using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Dtos;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Application.RawMaterialPO.Queries.GetNextRawMaterialPONumber
{
    public class GetNextRawMaterialPONumberQueryHandler : IRequestHandler<GetNextRawMaterialPONumberQuery, DocumentNumberPeekDto>
    {
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IRawMaterialPOQueryRepository _queryRepository;

        public GetNextRawMaterialPONumberQueryHandler(
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IRawMaterialPOQueryRepository queryRepository)
        {
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _queryRepository = queryRepository;
        }

        public async Task<DocumentNumberPeekDto> Handle(GetNextRawMaterialPONumberQuery request, CancellationToken cancellationToken)
        {
            var unitId = _ipAddressService.GetUnitId()
                ?? throw new InvalidOperationException("UnitId is not available for the current user.");

            var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeRMPO, MiscEnumEntity.ModulePurchase, unitId)
                ?? throw new InvalidOperationException("No transaction type configured for Raw Material Purchase Order.");

            // Peek the next number WITHOUT incrementing — the increment happens only on create/convert.
            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
            var nextNumber = sequences.LastOrDefault();

            var lastNumber = await _queryRepository.GetLastPONumberAsync();

            return new DocumentNumberPeekDto
            {
                LastNumber = lastNumber,
                NextNumber = nextNumber
            };
        }
    }
}
