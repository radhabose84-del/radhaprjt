#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrganisation.Commands.CreateSalesOrganisation
{
    public class CreateSalesOrganisationCommandHandler : IRequestHandler<CreateSalesOrganisationCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesOrganisationCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateSalesOrganisationCommandHandler(
            ISalesOrganisationCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesOrganisationCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesOrganisation>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALES_ORG_CREATE",
                actionName: request.SalesOrganisationCode,
                details: $"Sales Organisation '{request.SalesOrganisationCode}' created successfully with Id {newId}.",
                module: "SalesOrganisation"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Organisation created successfully.",
                Data = newId
            };
        }
    }
}
