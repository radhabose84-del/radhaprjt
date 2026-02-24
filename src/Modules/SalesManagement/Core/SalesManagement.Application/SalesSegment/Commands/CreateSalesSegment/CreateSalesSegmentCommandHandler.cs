#nullable disable

using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesSegment.Commands.CreateSalesSegment
{
    public class CreateSalesSegmentCommandHandler : IRequestHandler<CreateSalesSegmentCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesSegmentCommandRepository _commandRepository;
        private readonly ISalesSegmentQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateSalesSegmentCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesSegmentCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesSegment>(request);
            entity.IsActive = Domain.Common.BaseEntity.Status.Active;
            entity.IsDeleted = Domain.Common.BaseEntity.IsDelete.NotDeleted;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALES_SEGMENT_CREATE",
                actionName: request.SegmentName,
                details: $"Sales Segment '{request.SegmentName}' created successfully with Id {newId}.",
                module: "SalesSegment"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Segment created successfully.",
                Data = newId
            };
        }
    }
}
