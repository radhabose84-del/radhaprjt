#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrganisation.Commands.UpdateSalesOrganisation
{
    public class UpdateSalesOrganisationCommandHandler : IRequestHandler<UpdateSalesOrganisationCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesOrganisationCommandRepository _commandRepository;
        private readonly ISalesOrganisationQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSalesOrganisationCommandHandler(
            ISalesOrganisationCommandRepository commandRepository,
            ISalesOrganisationQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesOrganisationCommand request, CancellationToken cancellationToken)
        {
            var existing = await _queryRepository.GetByIdAsync(request.Id);
            if (existing == null)
                throw new EntityNotFoundException($"Sales Organisation with Id {request.Id} not found.");

            var entity = _mapper.Map<Domain.Entities.SalesOrganisation>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALES_ORG_UPDATE",
                actionName: existing.SalesOrganisationCode,
                details: $"Sales Organisation '{existing.SalesOrganisationCode}' updated successfully.",
                module: "SalesOrganisation"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Organisation updated successfully.",
                Data = updatedId
            };
        }
    }
}
