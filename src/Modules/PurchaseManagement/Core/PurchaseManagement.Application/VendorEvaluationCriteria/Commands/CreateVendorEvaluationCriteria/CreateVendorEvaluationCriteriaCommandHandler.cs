using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationCriteria.Commands.CreateVendorEvaluationCriteria
{
    public class CreateVendorEvaluationCriteriaCommandHandler : IRequestHandler<CreateVendorEvaluationCriteriaCommand, ApiResponseDTO<int>>
    {
        private readonly IVendorEvaluationCriteriaCommandRepository _commandRepository;
        private readonly IVendorEvaluationCriteriaQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateVendorEvaluationCriteriaCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(CreateVendorEvaluationCriteriaCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.VendorEvaluation.VendorEvaluationCriteria>(request);
            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "VENDOR_EVAL_CRITERIA_CREATE",
                actionName: request.CriteriaCode ?? string.Empty,
                details: $"VendorEvaluationCriteria '{request.CriteriaCode}' created successfully with Id {newId}.",
                module: "VendorEvaluationCriteria"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "VendorEvaluationCriteria created successfully.",
                Data = newId
            };
        }
    }
}
