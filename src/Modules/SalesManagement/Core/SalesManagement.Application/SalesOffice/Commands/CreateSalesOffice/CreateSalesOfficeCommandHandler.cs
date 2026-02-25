using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOffice.Commands.CreateSalesOffice
{
    public class CreateSalesOfficeCommandHandler : IRequestHandler<CreateSalesOfficeCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesOfficeCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateSalesOfficeCommandHandler(
            ISalesOfficeCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesOfficeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesOffice>(request);
            entity.IsActive = Domain.Common.BaseEntity.Status.Active;
            entity.IsDeleted = Domain.Common.BaseEntity.IsDelete.NotDeleted;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALES_OFFICE_CREATE",
                actionName: request.SalesOfficeName,
                details: $"Sales Office '{request.SalesOfficeName}' created successfully with Id {newId}.",
                module: "SalesOffice"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Office created successfully.",
                Data = newId
            };
        }
    }
}
