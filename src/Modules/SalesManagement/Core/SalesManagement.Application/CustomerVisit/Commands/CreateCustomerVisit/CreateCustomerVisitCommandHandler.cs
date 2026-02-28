using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.CustomerVisit.Commands.CreateCustomerVisit
{
    public class CreateCustomerVisitCommandHandler : IRequestHandler<CreateCustomerVisitCommand, ApiResponseDTO<int>>
    {
        private readonly ICustomerVisitCommandRepository _commandRepository;
        private readonly ICustomerVisitQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateCustomerVisitCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(CreateCustomerVisitCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.CustomerVisit>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "CUSTOMER_VISIT_CREATE",
                actionName: newId.ToString(),
                details: $"CustomerVisit created successfully with Id {newId}.",
                module: "CustomerVisit"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Customer Visit created successfully.",
                Data = newId
            };
        }
    }
}
