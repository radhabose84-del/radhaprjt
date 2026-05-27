using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationCriteria.Commands.UpdateVendorEvaluationCriteria
{
    public class UpdateVendorEvaluationCriteriaCommandHandler : IRequestHandler<UpdateVendorEvaluationCriteriaCommand, ApiResponseDTO<int>>
    {
        private readonly IVendorEvaluationCriteriaCommandRepository _commandRepository;
        private readonly IVendorEvaluationCriteriaQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateVendorEvaluationCriteriaCommandHandler(
            IVendorEvaluationCriteriaCommandRepository commandRepository,
            IVendorEvaluationCriteriaQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateVendorEvaluationCriteriaCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.VendorEvaluation.VendorEvaluationCriteria>(request);
            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "VENDOR_EVAL_CRITERIA_UPDATE",
                actionName: request.Id.ToString(),
                details: $"VendorEvaluationCriteria with Id {request.Id} updated successfully.",
                module: "VendorEvaluationCriteria"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "VendorEvaluationCriteria updated successfully.",
                Data = result
            };
        }
    }
}
