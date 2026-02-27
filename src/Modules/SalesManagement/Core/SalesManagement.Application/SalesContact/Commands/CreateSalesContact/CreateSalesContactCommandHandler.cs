using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesContact.Commands.CreateSalesContact
{
    public class CreateSalesContactCommandHandler : IRequestHandler<CreateSalesContactCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesContactCommandRepository _commandRepository;
        private readonly ISalesContactQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateSalesContactCommandHandler(
            ISalesContactCommandRepository commandRepository,
            ISalesContactQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesContactCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesContact>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALES_CONTACT_CREATE",
                actionName: request.ContactName ?? string.Empty,
                details: $"Sales Contact '{request.ContactName}' created successfully with Id {newId}.",
                module: "SalesContact"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Contact created successfully.",
                Data = newId
            };
        }
    }
}
