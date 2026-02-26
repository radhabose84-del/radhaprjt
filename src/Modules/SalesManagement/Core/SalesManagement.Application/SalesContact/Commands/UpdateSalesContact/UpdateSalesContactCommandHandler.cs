using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesContact.Commands.UpdateSalesContact
{
    public class UpdateSalesContactCommandHandler : IRequestHandler<UpdateSalesContactCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesContactCommandRepository _commandRepository;
        private readonly ISalesContactQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSalesContactCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesContactCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesContact>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALES_CONTACT_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Contact with Id {request.Id} updated successfully.",
                module: "SalesContact"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Contact updated successfully.",
                Data = result
            };
        }
    }
}
