using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.CustomerVisit.Commands.UpdateCustomerVisit
{
    public class UpdateCustomerVisitCommandHandler : IRequestHandler<UpdateCustomerVisitCommand, ApiResponseDTO<int>>
    {
        private readonly ICustomerVisitCommandRepository _commandRepository;
        private readonly ICustomerVisitQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateCustomerVisitCommandHandler(
            ICustomerVisitCommandRepository commandRepository,
            ICustomerVisitQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateCustomerVisitCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.CustomerVisit>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "CUSTOMER_VISIT_UPDATE",
                actionName: request.Id.ToString(),
                details: $"CustomerVisit with Id {request.Id} updated successfully.",
                module: "CustomerVisit"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Customer Visit updated successfully.",
                Data = result
            };
        }
    }
}
