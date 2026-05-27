using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Domain.Entities.VendorEvaluation;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Commands.UpdateVendorEvaluationHeader
{
    public class UpdateVendorEvaluationHeaderCommandHandler : IRequestHandler<UpdateVendorEvaluationHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IVendorEvaluationHeaderCommandRepository _commandRepository;
        private readonly IVendorEvaluationHeaderQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateVendorEvaluationHeaderCommandHandler(
            IVendorEvaluationHeaderCommandRepository commandRepository,
            IVendorEvaluationHeaderQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateVendorEvaluationHeaderCommand request, CancellationToken cancellationToken)
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

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "VENDOR_EVAL_HEADER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"VendorEvaluationHeader with Id {request.Id} updated successfully.",
                module: "VendorEvaluationHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "VendorEvaluationHeader updated successfully.",
                Data = result
            };
        }
    }
}
