#nullable disable
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
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSalesSegmentCommandHandler(
            ISalesSegmentCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesSegmentCommand request, CancellationToken cancellationToken)
        {
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
