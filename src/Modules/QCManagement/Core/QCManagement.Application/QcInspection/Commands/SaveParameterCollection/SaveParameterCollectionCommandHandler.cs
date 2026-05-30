using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.Common.Services;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QcInspection.Commands.SaveParameterCollection
{
    public class SaveParameterCollectionCommandHandler : IRequestHandler<SaveParameterCollectionCommand, ApiResponseDTO<int>>
    {
        private readonly IQcInspectionCommandRepository _commandRepository;
        private readonly IQcInspectionQueryRepository _queryRepository;
        private readonly IInspectionEvaluator _evaluator;
        private readonly IMediator _mediator;

        public SaveParameterCollectionCommandHandler(
            IQcInspectionCommandRepository commandRepository,
            IQcInspectionQueryRepository queryRepository,
            IInspectionEvaluator evaluator,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _evaluator = evaluator;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(SaveParameterCollectionCommand request, CancellationToken cancellationToken)
        {
            var evalRows = (await _queryRepository.GetDetailEvaluationRowsAsync(request.QcInspectionHdrId))
                .ToDictionary(r => r.Id);

            var results = new List<(int DetailId, string? ActualValue, string? InspectionResult, string? Remarks)>();

            foreach (var p in request.Parameters)
            {
                evalRows.TryGetValue(p.DetailId, out var rule);
                var inspectionResult = _evaluator.Evaluate(
                    rule?.ValidationTypeCode, p.ActualValue,
                    rule?.MinValue, rule?.MaxValue, rule?.ExpectedValue, rule?.AllowedValues);

                results.Add((p.DetailId, p.ActualValue, inspectionResult, p.Remarks));
            }

            await _commandRepository.SaveParameterResultsAsync(request.QcInspectionHdrId, results);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "QC_INSPECTION_PARAMETERS_SAVE",
                actionName: request.QcInspectionHdrId.ToString(),
                details: $"Parameter results saved for QC Inspection {request.QcInspectionHdrId} ({results.Count} parameter(s)).",
                module: "QcInspection"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Parameter results saved successfully.",
                Data = request.QcInspectionHdrId
            };
        }
    }
}
