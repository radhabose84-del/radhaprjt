using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder
{
    public class CreateSalesOrderCommandHandler : IRequestHandler<CreateSalesOrderCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesOrderCommandRepository _commandRepository;
        private readonly ISalesOrderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateSalesOrderCommandHandler(
            ISalesOrderCommandRepository commandRepository,
            ISalesOrderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<SalesOrderHeader>(request.SalesOrderDetails);

            // Generate auto-number
            var salesOrderNo = await _commandRepository.GenerateNextSalesOrderNoAsync(
                request.SalesOrderDetails!.UnitId, cancellationToken);
            entity.SalesOrderNo = salesOrderNo;
            entity.OrderDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALESORDER_CREATE",
                actionName: salesOrderNo,
                details: $"Sales Order '{salesOrderNo}' created successfully with Id {newId}.",
                module: "SalesOrder");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Order created successfully.",
                Data = newId
            };
        }
    }
}
