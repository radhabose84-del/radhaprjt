using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.VendorEvaluation;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Commands.CreateVendorEvaluationHeader
{
    public class CreateVendorEvaluationHeaderCommandHandler : IRequestHandler<CreateVendorEvaluationHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IVendorEvaluationHeaderCommandRepository _commandRepository;
        private readonly IVendorEvaluationHeaderQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;

        public CreateVendorEvaluationHeaderCommandHandler(
            IVendorEvaluationHeaderCommandRepository commandRepository,
            IVendorEvaluationHeaderQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateVendorEvaluationHeaderCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.VendorEvaluation.VendorEvaluationHeader>(request);

            // Map detail items
            if (request.Details != null && request.Details.Count > 0)
            {
                entity.VendorEvaluationDetails = request.Details.Select(d => new VendorEvaluationDetail
                {
                    CriteriaId = d.CriteriaId,
                    Score = d.Score,
                    WeightagePercent = d.WeightagePercent,
                    WeightedScore = d.WeightedScore,
                    ScoringMethod = d.ScoringMethod,
                    Remarks = d.Remarks
                }).ToList();
            }

            // Generate EvaluationCode from DocumentSequence
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeVendorEvaluation, MiscEnumEntity.ModulePurchase, unitId)
                ?? throw new InvalidOperationException("No transaction type configured for Vendor Evaluation.");
            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
            entity.EvaluationCode = sequences.Count > 0
                ? sequences[^1]
                : throw new InvalidOperationException("No document sequence configured for Vendor Evaluation.");

            var newId = await _commandRepository.CreateAsync(entity, transactionTypeId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "VENDOR_EVAL_HEADER_CREATE",
                actionName: entity.EvaluationCode,
                details: $"VendorEvaluationHeader '{entity.EvaluationCode}' created successfully with Id {newId}.",
                module: "VendorEvaluationHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "VendorEvaluationHeader created successfully.",
                Data = newId
            };
        }
    }
}
