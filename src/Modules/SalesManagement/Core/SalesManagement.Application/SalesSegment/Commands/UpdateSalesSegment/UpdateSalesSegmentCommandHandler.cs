using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesSegment.Commands.UpdateSalesSegment
{
    public class UpdateSalesSegmentCommandHandler : IRequestHandler<UpdateSalesSegmentCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesSegmentCommandRepository _commandRepository;
        private readonly ISalesSegmentQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSalesSegmentCommandHandler(
            ISalesSegmentCommandRepository commandRepository,
            ISalesSegmentQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesSegmentCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsSalesSegmentLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<Domain.Entities.SalesSegment>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALES_SEGMENT_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Segment with Id {request.Id} updated successfully.",
                module: "SalesSegment"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Segment updated successfully.",
                Data = result
            };
        }
    }
}
