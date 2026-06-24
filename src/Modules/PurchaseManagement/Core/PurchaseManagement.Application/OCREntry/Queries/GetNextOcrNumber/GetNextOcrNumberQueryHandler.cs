using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Dtos;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Application.OCREntry.Queries.GetNextOcrNumber
{
    public class GetNextOcrNumberQueryHandler : IRequestHandler<GetNextOcrNumberQuery, DocumentNumberPeekDto>
    {
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IOCREntryQueryRepository _queryRepository;

        public GetNextOcrNumberQueryHandler(
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IOCREntryQueryRepository queryRepository)
        {
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _queryRepository = queryRepository;
        }

        public async Task<DocumentNumberPeekDto> Handle(GetNextOcrNumberQuery request, CancellationToken cancellationToken)
        {
            var unitId = _ipAddressService.GetUnitId()
                ?? throw new InvalidOperationException("UnitId is not available for the current user.");

            var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeOCR, MiscEnumEntity.ModulePurchase, unitId)
                ?? throw new InvalidOperationException("No transaction type configured for OCR.");

            // Peek the next number WITHOUT incrementing — the increment happens only on create.
            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
            var nextNumber = sequences.LastOrDefault();

            var lastNumber = await _queryRepository.GetLastOcrNumberAsync();

            return new DocumentNumberPeekDto
            {
                LastNumber = lastNumber,
                NextNumber = nextNumber
            };
        }
    }
}
